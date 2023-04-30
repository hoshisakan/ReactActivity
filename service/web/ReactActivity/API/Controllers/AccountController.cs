using API.DTOs;
using API.Services;
using Application.Core;
using Application.Module;
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
        private readonly TokenService _tokenService;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly ILogger<AppUser> _logger;


        public AccountController(UserManager<AppUser> userManager, TokenService tokenService,
            IDistributedCache cache, IConfiguration config, ILogger<AppUser> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _cache = cache;
            _config = config;
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

            bool result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (result)
            {
                UserDto? userDto = new UserDto();

                CacheTokenDto? decryptCacheTokenDto = await DecryptCacheItem<CacheTokenDto>(user.UserName);

                if (decryptCacheTokenDto != null)
                {
                    _logger.LogInformation($"username: {user.UserName}, token: {decryptCacheTokenDto.Token}, expires_at: {decryptCacheTokenDto.ExpiresIn}");
                    userDto = await CreateUserObject(user, decryptCacheTokenDto);
                }
                else
                {
                    _logger.LogInformation($"username: {user.UserName}, token: null, expires_at: null");
                    userDto = await CreateUserObject(user, true);
                    CacheTokenDto cacheTokenDto = new CacheTokenDto{
                        AppUserId = user.Id,
                        Username = user.UserName,
                        Token = userDto.Token,
                        ExpiresIn = userDto.ExpiresIn
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

            return await CreateUserObject(user);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<UserDto>> RefreshToken(RequestTokenDto requestTokenDto)
        {
            try {
                string accessToken = requestTokenDto.AccessToken;
                string refreshToken = requestTokenDto.RefreshToken;

                ValidateTokenDto validateTokenDto = _tokenService.RecoveryToken(accessToken);

                string JwtId = validateTokenDto.JwtId;
                bool IsValid = validateTokenDto.IsValid;
                bool IsExpired = validateTokenDto.IsExpired;

                if (!IsValid)
                {
                    return BadRequest("Invalid access token");
                }

                if (IsValid && !IsExpired)
                {
                    return BadRequest("Token has not yet expired");
                }

                _logger.LogInformation($"Old access token validation pass, JwtId: {JwtId}");

                RefreshTokenDto refreshTokenDto = new RefreshTokenDto();
                Result<RefreshTokenDto>? response = await Mediator.Send(
                    new Retrieve.Query {
                        RefreshToken = requestTokenDto.RefreshToken,
                        Predicate = "token"
                    }
                );

                refreshTokenDto = response.Value ?? new RefreshTokenDto();

                if (string.IsNullOrEmpty(refreshTokenDto.Token))
                {
                    return BadRequest("Invalid refresh token.");
                }

                _logger.LogInformation($"Retrieve refresh token: {refreshTokenDto.Token}");
                _logger.LogInformation($"Request refresh token: {requestTokenDto.RefreshToken}");
                _logger.LogInformation($"Retrieve refresh token expires time: {refreshTokenDto.ExpirationTime}");

                if (DateTimeTool.CompareBothOfTime(DateTime.Now, refreshTokenDto.ExpirationTime) > 0)
                {
                    return BadRequest("Refresh token has been expired, please re-login.");
                }

                AppUser? user = await _userManager.Users.Include(p => p.Photos)
                    .FirstOrDefaultAsync(x => x.Id == refreshTokenDto.AppUserId);

                if (user == null)
                {
                    return BadRequest("Refresh token can't be reference to any user.");
                }
                _logger.LogInformation($"Current request refresh token user: {user.UserName}");

                UserDto userDto = await CreateUserObject(user);

                _logger.LogInformation($"Current token owner: {userDto.Username}");
                _logger.LogInformation($"New access token: {userDto.Token}");
                _logger.LogInformation($"New access token expires time: {userDto.ExpiresIn}");
                _logger.LogInformation($"Current refresh token: {userDto.RefreshToken}");

                CacheTokenDto cacheTokenDto = new CacheTokenDto{
                    AppUserId = user.Id,
                    Username = user.UserName,
                    Token = userDto.Token,
                    ExpiresIn = userDto.ExpiresIn
                };
                await SaveTokenToCache(user.UserName, cacheTokenDto);

                userDto.RefreshToken = refreshTokenDto.Token;

                await Mediator.Send(
                    new UpdateUsedState.Command {
                        AppUserId = user.Id,
                        Token = refreshTokenDto.Token
                    }
                );

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Message: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("revoke/{username}")]
        public async Task<IActionResult> Revoke(string username)
        {
            AppUser? user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return BadRequest("Invalid username.");
            }
            _logger.LogInformation($"Username: {username}, AppUserId: {user.Id}.");
            return HandleResult(await Mediator.Send(new Revoke.Command { AppUserId = user.Id }));
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

        private async Task<UserDto> CreateUserObject(AppUser user, bool reset = false)
        {
            TokenDto token = await _tokenService.CreateToken(user, reset);

            return new UserDto
            {
                DisplayName = user.DisplayName,
                Image = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                Token = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresIn = token.ExpiresIn,
                Username = user.UserName
            };
        }

        private async Task<UserDto> CreateUserObject(AppUser user, CacheTokenDto cacheTokenDto)
        {
            UserDto userDto = new UserDto(){
                DisplayName = user.DisplayName,
                Image = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                Token = cacheTokenDto.Token,
                RefreshToken = null,
                ExpiresIn = cacheTokenDto.ExpiresIn,
                Username = user.UserName
            };
            try {
                Result<RefreshTokenDto>? response = await Mediator.Send(
                    new Retrieve.Query {
                        AppUserId = user.Id,
                        Predicate = "all"
                    }
                );
                RefreshTokenDto? refreshTokenDto = new RefreshTokenDto();

                if (response == null)
                {
                    throw new Exception("Instance of response is null.");
                }
                else
                {
                    _logger.LogInformation($"Instance of response valid.");
                    userDto.RefreshToken = response.Value?.Token ?? throw new ArgumentNullException(nameof(response.Value));
                    refreshTokenDto = response.Value ?? throw new ArgumentNullException(nameof(response.Value));
                }
            } catch (Exception ex) {
                _logger.LogInformation($"Error: {ex.Message}");
            }
            return userDto;
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
            int requestAccessTokenExpiresTime = _config.GetSection("JWTSettings:AccessTokenExpirationTime").Get<int?>() ?? -1;

            if (requestAccessTokenExpiresTime == -1)
            {
                throw new Exception("Access token expires time is not set.");
            }

            string cacheTokenJson = JsonSerializer.Serialize(cacheTokenDto);
            //TODO: Set cache, but don't limit expiration time.
            // await _cache.SetStringAsync(cacheKey, cacheTokenJson);
            //TODO: Set cache and add expiration time.
            await _cache.SetStringAsync(
                cacheKey, cacheTokenJson,
                new DistributedCacheEntryOptions{
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(requestAccessTokenExpiresTime)
                    // AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(requestAccessTokenExpiresTime)
                }
            );
        }
    
    }
}