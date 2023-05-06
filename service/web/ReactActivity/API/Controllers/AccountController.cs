using API.DTOs;
using API.Services;
using Application.RefreshTokens;
using Domain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly TokenService _tokenService;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AppUser> _logger;


        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
            TokenService tokenService, IDistributedCache cache, IConfiguration config, ILogger<AppUser> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenService = tokenService;
            _cache = cache;
            _config = config;
            string baseAddress = _config["API:BaseAddress"] ?? string.Empty;
            if (string.IsNullOrEmpty(baseAddress))
            {
                throw new Exception("API:BaseAddress is not set.");
            }
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            AppUser? user = await _userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.Email == loginDto.Email);

            if (user == null)
            {
                _logger.LogInformation("User not found.");
                return Unauthorized();
            }

            Microsoft.AspNetCore.Identity.SignInResult? result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (result.Succeeded)
            {
                _logger.LogInformation($"username: {user.UserName}");
                UserDto? userDto = new UserDto();
                
                await SetRefreshToken(user);

                CacheTokenDto? decryptCacheTokenDto = await DecryptCacheItem<CacheTokenDto>(user.UserName);

                if (decryptCacheTokenDto != null)
                {
                    _logger.LogInformation($"username: {user.UserName}, token: {decryptCacheTokenDto.Token}");
                    userDto = await CreateUserObject(user, true);
                }
                else
                {
                    _logger.LogInformation($"username: {user.UserName}, token: null, expires_at: null");
                    userDto = await CreateUserObject(user, false);
                    CacheTokenDto cacheTokenDto = new CacheTokenDto{
                        AppUserId = user.Id,
                        Username = user.UserName,
                        Token = userDto.Token
                    };
                    await SaveTokenToCache(user.UserName, cacheTokenDto);
                }
                return userDto;
            }
            return Unauthorized();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await _userManager.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                ModelState.AddModelError("email", "Email is already taken.");
                return ValidationProblem();
            }

            if (await _userManager.Users.AnyAsync(u => u.UserName == registerDto.Username))
            {
                ModelState.AddModelError("username", "Username is already taken.");
                return ValidationProblem();
            }

            AppUser user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Username
            };

            _logger.LogInformation(
                "User: " + user.DisplayName + " " + user.Email + " " + user.UserName
            );

            IdentityResult? result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                await SetRefreshToken(user);
                return await CreateUserObject(user);
            }
            return BadRequest(result.Errors);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            AppUser? user = await _userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.Email == User.FindFirstValue(ClaimTypes.Email));
            
            if (user == null)
            {
                return Unauthorized();
            }
            await SetRefreshToken(user);
            return await CreateUserObject(user);
        }

        [AllowAnonymous]
        [HttpPost("fbLogin")]
        public async Task<ActionResult<UserDto>> FacebookLogin(string accessToken)
        {
            string fbVerifyKeys = _config["Facebook:AppId"] + "|" + _config["Facebook:ApiSecret"];
            HttpResponseMessage? verifyToken = await _httpClient.GetAsync($"debug_token?input_token={accessToken}&access_token={fbVerifyKeys}");
            if (!verifyToken.IsSuccessStatusCode) // Not equal to 200
            {
                return Unauthorized();
            }

            string fbUrl = $"me?access_token={accessToken}&fields=name,email,picture.width(100).height(100)";
            FacebookDto? fbInfo = await _httpClient.GetFromJsonAsync<FacebookDto>(fbUrl);
            
            if (fbInfo == null)
            {
                return Unauthorized();
            }

            AppUser? user = await _userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.Email == fbInfo.Email);

            if (user != null)
            {
                return await CreateUserObject(user);
            }

            user = new AppUser
            {
                DisplayName = fbInfo.Name,
                Email = fbInfo.Email,
                // UserName = fbInfo.Email.Split("@")[0],
                UserName = fbInfo.Email,
                Photos = new List<Photo>
                {
                    new Photo
                    {
                        Id = "fb_" + fbInfo.Id,
                        Url = fbInfo.Picture.Data.Url,
                        IsMain = true
                    }
                }
            };

            IdentityResult? result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest("Problem creating user account.");
            }

            await SetRefreshToken(user);

            return await CreateUserObject(user);
        }

        [Authorize]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<UserDto>> RefreshToken()
        {
            try {
                string? refreshToken = Request.Cookies["refreshToken"];

                AppUser? user = await _userManager.Users
                    .Include(r => r.RefreshTokens)
                    .Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.UserName == User.FindFirstValue(ClaimTypes.Name))
                ;

                if (user == null)
                {
                    _logger.LogInformation($"RefreshToken request from: {User.FindFirstValue(ClaimTypes.Name)}");
                    return Unauthorized();
                }

                _logger.LogInformation($"RefreshToken request from: {user.RefreshTokens.Count}");
                _logger.LogInformation($"RefreshToken request from: {user.UserName}");
                _logger.LogInformation($"RefreshToken request from: {user.Email}");

                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.LogInformation($"Invalid refresh token: {refreshToken}");
                    return Unauthorized();
                }

                RefreshToken? oldToken = user.RefreshTokens.SingleOrDefault(x => x.Token == refreshToken);

                if (oldToken != null && !oldToken.IsActive)
                {
                    _logger.LogInformation($"Invalid refresh token: {oldToken.Token}");
                    return Unauthorized();
                }
                _logger.LogInformation($"Read oldToken: {oldToken?.Token}");
                _logger.LogInformation($"Read session refreshToken: {refreshToken}");
                UserDto? userNewTokenDto = await CreateUserObject(user);
                _logger.LogInformation($"CreateUserObject: {userNewTokenDto.Token}");
                CacheTokenDto cacheTokenDto = new CacheTokenDto{
                    AppUserId = user.Id,
                    Username = user.UserName,
                    Token = userNewTokenDto.Token
                };
                await SaveTokenToCache(user.UserName, cacheTokenDto);
                return userNewTokenDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Message: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return Unauthorized();
            }
        }

        [AllowAnonymous]
        [HttpPost("verity-token")]
        public IActionResult VerifyToken(VerityTokenDto verityTokenDto)
        {
            string accessToken = verityTokenDto.AccessToken;
            _logger.LogInformation($"Verify token is: {accessToken}");

            bool validateResult = _tokenService.VerifyToken(accessToken);

            if (validateResult)
            {
                return Ok();
            }
            return BadRequest("Invalid access token.");
        }

        private async Task<UserDto> CreateUserObject(AppUser user, bool cacheExistsToken = false)
        {
            string token = string.Empty;

            if (cacheExistsToken)
            {
                token = await _cache.GetStringAsync(user.UserName);
            }
            else
            {
                token = _tokenService.CreateToken(user);
            }

            return new UserDto
            {
                DisplayName = user.DisplayName,
                Image = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                Token = token,
                Username = user.UserName
            };
        }

        private async Task<T> DecryptCacheItem<T>(string cacheKey)
        {
            string? decryptCacheToken = await _cache.GetStringAsync(cacheKey);
            if (decryptCacheToken == null)
            {
                _logger.LogInformation($"Cache token is null, error key: {cacheKey}.");
                return await Task.FromResult<T>(default(T));
            }

            T? result = JsonSerializer.Deserialize<T>(decryptCacheToken);

            if (result == null)
            {
                _logger.LogInformation("Deserializer Cache token failed.");
                throw new ArgumentNullException("Deserializer Cache token failed.");
            }
            return await Task.FromResult(result);
        }

        private async Task SaveTokenToCache(string cacheKey, CacheTokenDto cacheTokenDto)
        {
            int requestAccessTokenExpiresTime = _config.GetSection("JWTSettings:AccessTokenExpiresTime").Get<int?>() ?? -1;

            if (requestAccessTokenExpiresTime == -1)
            {
                throw new Exception("Access token expires time is not set.");
            }

            string cacheTokenJson = JsonSerializer.Serialize(cacheTokenDto);
            //TODO: Set cache and add expiration time.
            await _cache.SetStringAsync(
                cacheKey, cacheTokenJson,
                new DistributedCacheEntryOptions{
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(requestAccessTokenExpiresTime)
                }
            );
        }

        private async Task<bool> VerifyClientRefreshToken(AppUser authenticatedUser)
        {
            string? sessionRefreshToken = Request.Cookies["refreshToken"];

            try {
                if (string.IsNullOrEmpty(sessionRefreshToken))
                {
                    _logger.LogInformation($"Session refresh token is null.");
                    return await Task.FromResult(false);
                }
                else
                {
                    _logger.LogInformation($"User Id: {authenticatedUser.Id}, authenticatedUser refresh token: {sessionRefreshToken}");

                    Application.Core.Result<RefreshTokenDto>? queryResult = await Mediator.Send(
                        new Retrieve.Query {
                            AppUserId = authenticatedUser.Id,
                            Token = sessionRefreshToken,
                            Predicate = "all"
                        }
                    );
                    RefreshTokenDto? refreshTokenDto = new RefreshTokenDto();
                    refreshTokenDto = queryResult.Value;

                    if (refreshTokenDto == null)
                    {
                        _logger.LogInformation($"Refresh token is null, not found match token.");
                        return await Task.FromResult(false);
                    }

                    _logger.LogInformation($"Refresh token is not null.");
                    _logger.LogInformation($"Refresh token: {refreshTokenDto.Token}");
                    _logger.LogInformation($"Refresh IsActive: {refreshTokenDto.IsActive}");
                    _logger.LogInformation($"Refresh IsExpired: {refreshTokenDto.IsExpired}");

                    if (refreshTokenDto.IsActive == false)
                    {
                        _logger.LogInformation($"Refresh token is not active.");
                        return await Task.FromResult(false);
                    }

                    if (refreshTokenDto.IsExpired == true)
                    {
                        _logger.LogInformation($"Refresh token is expired.");
                        return await Task.FromResult(false);
                    }
                    return await Task.FromResult(true);
                }
            } catch (Exception ex) {
                _logger.LogInformation($"Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return await Task.FromResult(false);
            }
        }
    
        private async Task SetRefreshToken(AppUser user)
        {
            bool verified = VerifyClientRefreshToken(user).Result;

            if (verified)
            {
                _logger.LogInformation($"Refresh token is verified.");
                return;
            }

            int requestRefreshTokenExpiresTime = _config.GetSection("JWTSettings:RefreshTokenExpiresTime").Get<int?>() ?? -1;

            if (requestRefreshTokenExpiresTime == -1)
            {
                throw new Exception("Refresh token expires time is not set.");
            }

            RefreshToken refreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);
            CookieOptions cookieOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                IsEssential = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
            string? sessionRefreshToken = Request.Cookies["refreshToken"];
            _logger.LogInformation($"Session refresh token: {sessionRefreshToken}");
        }
    }
}