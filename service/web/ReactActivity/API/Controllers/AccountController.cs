using API.DTOs;
using API.Services;
using Application.Core;
using Application.RefreshTokens;
using Domain;
using Infrastructure.Email;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text;
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
        private readonly ILogger<AppUser> _logger;
        private readonly EmailSender _emailSender;
        private string _baseAddress;
        private HttpClient _httpClient = new HttpClient();


        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
            TokenService tokenService, IDistributedCache cache, IConfiguration config, ILogger<AppUser> logger,
            EmailSender emailSender)
        {
            _emailSender = emailSender;
            _signInManager = signInManager;
            _userManager = userManager;
            _tokenService = tokenService;
            _cache = cache;
            _config = config;
            string baseAddress = _config["API:BaseAddress"] ?? string.Empty;
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
                _logger.LogInformation("Invalid email.");
                return Unauthorized("Invalid email.");
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogInformation("Email not confirmed.");
                return Unauthorized("Email not confirmed.");
            }

            Microsoft.AspNetCore.Identity.SignInResult? result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (result.Succeeded)
            {
                
                _logger.LogInformation($"username: {user.UserName}");
                UserDto? userDto = new UserDto();
                userDto = await CreateUserObject(user, true);
                await SetRefreshToken(user);
                return userDto;
            }
            return Unauthorized("Invalid password");
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            try {
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

                if (!result.Succeeded)
                {
                    _logger.LogInformation("Problem registering user.");
                    return BadRequest("Problem registering user.");
                }

                await SendVerifyEmail(user, "register");

                return Ok("Registration successful - please check your email to verify your email address.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error message: {ex.Message}\nerror stack: {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("verifyEmail")]
        public async Task<IActionResult> VerifyEmail(string token, string email)
        {
            try {
                AppUser? user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BadRequest("Invalid email address.");
                }

                byte[] decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
                string decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                IdentityResult? result = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (!result.Succeeded)
                {
                    return BadRequest("Invalid token, Cloud not verify email address.");
                }
                return Ok("Email confirmed - you can now login.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error message: {ex.Message}\nerror stack: {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("resendEmailConfirmationLink")]
        public async Task<IActionResult> ResendEmailConfirmationLink(string email)
        {
            AppUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return BadRequest("Invalid email address.");
            }

            await SendVerifyEmail(user, "register");

            return Ok("Email verification link resent.");
        }

        [AllowAnonymous]
        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgetPasswordDto forgetPasswordDto)
        {
            _logger.LogInformation($"email: {forgetPasswordDto.Email}");

            AppUser? user = await _userManager.FindByEmailAsync(forgetPasswordDto.Email);

            if (user == null)
            {
                return BadRequest("Invalid email address.");
            }

            await SendVerifyEmail(user, "forgotPassword");

            return Ok("Password reset link sent.");
        }

        [AllowAnonymous]
        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            try {
                AppUser? user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);

                if (user == null)
                {
                    return BadRequest("Invalid email address.");
                }

                byte[] decodedTokenBytes = WebEncoders.Base64UrlDecode(resetPasswordDto.Token);
                string decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                IdentityResult? result = await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordDto.Password);

                if (!result.Succeeded)
                {
                    return BadRequest("Invalid token, Cloud not reset password.");
                }
                return Ok("Password reset successful - you can now login.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error message: {ex.Message}\nerror stack: {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
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
            return await CreateUserObject(user, true);
        }

        [AllowAnonymous]
        [HttpPost("fbLogin")]
        public async Task<ActionResult<UserDto>> FacebookLogin(string accessToken)
        {
            _baseAddress = _config["API:Facebook:BaseAddress"] ?? throw new ArgumentNullException("Facebook API base address is null.");
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseAddress)
            };
            string fbVerifyKeys = _config["Facebook:AppId"] + "|" + _config["Facebook:ApiSecret"];
            HttpResponseMessage? verifyToken = await _httpClient.GetAsync($"debug_token?input_token={accessToken}&access_token={fbVerifyKeys}");
            if (!verifyToken.IsSuccessStatusCode) // Not equal to 200
            {
                return Unauthorized("Invalid facebook token.");
            }

            string fbUrl = $"me?access_token={accessToken}&fields=name,email,picture.width(100).height(100)";
            FacebookDto? fbInfo = await _httpClient.GetFromJsonAsync<FacebookDto>(fbUrl);
            
            if (fbInfo == null)
            {
                return Unauthorized("Problem authenticating with facebook.");
            }

            AppUser? user = await _userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.Email == fbInfo.Email);

            if (user != null)
            {
                _logger.LogInformation($"username: {user.UserName} already exists, creating new token.");
                await SetRefreshToken(user);
                return await CreateUserObject(user, true);
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
                _logger.LogInformation("Problem creating user account.");
                return BadRequest("Problem creating user account.");
            }
            _logger.LogInformation($"username: {user.UserName} created, creating new token.");
            await SetRefreshToken(user);

            return await CreateUserObject(user, false);
        }

        [AllowAnonymous]
        [HttpPost("googleLogin")]
        public async Task<ActionResult<UserDto>> GoogleLogin(string accessToken)
        {
            _baseAddress = _config["API:Google:BaseAddress"] ?? throw new ArgumentNullException("Google API base address is null.");
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseAddress)
            };
            HttpResponseMessage? verifyToken = await _httpClient.GetAsync($"tokeninfo?access_token={accessToken}");
            if (!verifyToken.IsSuccessStatusCode) // Not equal to 200
            {
                return Unauthorized("Invalid google token.");
            }

            string googleInfoUrl = $"userinfo?access_token={accessToken}";
            GoogleDto? googleInfo = await _httpClient.GetFromJsonAsync<GoogleDto>(googleInfoUrl);
            
            if (googleInfo == null)
            {
                return Unauthorized("Problem authenticating with google.");
            }

            AppUser? user = await _userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(x => x.Email == googleInfo.Email);

            if (user != null)
            {
                _logger.LogInformation($"username: {user.UserName} already exists, creating new token.");
                await SetRefreshToken(user);
                return await CreateUserObject(user, true);
            }

            user = new AppUser
            {
                DisplayName = googleInfo.Name,
                Email = googleInfo.Email,
                // UserName = fbInfo.Email.Split("@")[0],
                UserName = googleInfo.Email,
                Photos = new List<Photo>
                {
                    new Photo
                    {
                        Id = "google_" + googleInfo.Sub,
                        Url = googleInfo.Picture,
                        IsMain = true
                    }
                }
            };

            IdentityResult? result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogInformation("Problem creating user account.");
                return BadRequest("Problem creating user account.");
            }
            _logger.LogInformation($"username: {user.UserName} created, creating new token.");
            await SetRefreshToken(user);

            return await CreateUserObject(user, false);
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<ActionResult<LogoutResponseDto>> Logout(LogoutRequestDto logoutRequestDto)
        {
            LogoutResponseDto logoutDto = new LogoutResponseDto();
            try
            {
                AppUser? user = await _userManager.Users
                    .Include(r => r.RefreshTokens)
                    .FirstOrDefaultAsync(x => x.UserName == logoutRequestDto.Username);

                if (user == null)
                {
                    _logger.LogInformation("User not found.");
                    throw new InvalidOperationException("User not found.");
                }

                _logger.LogInformation($"username: {user.UserName}, access token: {logoutRequestDto.Token}");

                Result<Unit>? revokedResult = await Mediator.Send(
                    new Revoke.Command {
                        AppUserId = user.Id
                    }
                );

                if (revokedResult.IsSuccess)
                {
                    logoutDto = new LogoutResponseDto
                    {
                        Message = "Logout successfully",
                        IsLogout = true,
                        Error = string.Empty
                    };
                }
                else
                {
                    logoutDto = new LogoutResponseDto
                    {
                        Message = "Logout failed, force logout",
                        IsLogout = true,
                        Error = revokedResult.Error
                    };
                }
                Response.Cookies.Delete("refreshToken");
                await _cache.RemoveAsync(user.UserName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error message: {ex.Message}\nerror stack: {ex.StackTrace}");
                logoutDto = new LogoutResponseDto
                {
                    Message = "Logout failed",
                    IsLogout = false,
                    Error = ex.Message
                };
            }
            return logoutDto;
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
                    // .Where(u => u.RefreshTokens.Any(t => t.Revoked == null))
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
                UserDto? userNewTokenDto = await CreateUserObject(user, false);
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

        //TODO: Need to fix this, because any user can revoke any user's token,
        //TODO: Need to check if the user is the owner of the token, or admin
        //TODO: If the user is admin, then can revoke any user's token, otherwise, only revoke his own token
        [AllowAnonymous]
        [HttpPost("revoke-token/{username}")]
        public async Task<IActionResult> RevokeRefreshToken(string username)
        {
            try
            {
                string? refreshToken = Request.Cookies["refreshToken"];
                _logger.LogInformation($"Revoke token is: {refreshToken}");

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return BadRequest("Invalid refresh token.");
                }

                _logger.LogInformation($"Revoke token request user is: {username}");

                AppUser? user = _userManager.Users
                    .Include(r => r.RefreshTokens)
                    .FirstOrDefault(x => x.UserName == username)
                ;

                if (user == null)
                {
                    return Unauthorized();
                }

                _logger.LogInformation($"Will starting user id: {user.Id} remove refresh token: {refreshToken}");

                Result<Unit>? revokedResult = await Mediator.Send(
                    new Revoke.Command {
                        AppUserId = user.Id
                    }
                );
                return HandleResult(revokedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Message: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        private async Task<string> GetNewAccessToken(AppUser user)
        {
            string token = string.Empty;
            token = _tokenService.CreateToken(user);
            CacheTokenDto newCacheTokenDto = new CacheTokenDto{
                AppUserId = user.Id,
                Username = user.UserName,
                Token = token
            };
            await SaveTokenToCache(user.UserName, newCacheTokenDto);
            return token;
        }

        private async Task<UserDto> CreateUserObject(AppUser user, bool requestCheckCache)
        {
            string token = string.Empty;
            UserDto userDto = new UserDto();

            if (requestCheckCache)
            {
                CacheTokenDto? decryptCacheTokenDto = await DecryptCacheItem<CacheTokenDto>(user.UserName);
                
                if (decryptCacheTokenDto == null)
                {
                    _logger.LogInformation($"Cache token is null, error key: {user.UserName}.");
                    token = await GetNewAccessToken(user);
                }
                else
                {
                    token = decryptCacheTokenDto.Token;
                }
                userDto = new UserDto
                {
                    DisplayName = user.DisplayName,
                    Image = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                    Token = token,
                    Username = user.UserName
                };
            }
            else
            {
                token = await GetNewAccessToken(user);
                _logger.LogInformation($"User: {user.UserName}, displayName: {user.DisplayName}, create access token: {token}");

                Photo? userMainPhoto = user.Photos?.Where(x => x.IsMain).FirstOrDefault();
                string? userMainPhotoUrl = userMainPhoto?.Url;

                userDto = new UserDto
                {
                    DisplayName = user.DisplayName,
                    Image = userMainPhotoUrl,
                    Token = token,
                    Username = user.UserName
                };
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
            int requestAccessTokenExpiresTime = _config.GetSection("JWTSettings:AccessTokenExpiresTime").Get<int?>() ?? -1;

            if (requestAccessTokenExpiresTime == -1)
            {
                throw new Exception("Access token expires time is not set.");
            }

            string cacheTokenJson = JsonSerializer.Serialize<CacheTokenDto>(cacheTokenDto);
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
                    
                    return await Task.FromResult(true);
                }
            } catch (Exception ex)
            {
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
                Expires = DateTime.UtcNow.AddDays(requestRefreshTokenExpiresTime)
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
            string? sessionRefreshToken = Request.Cookies["refreshToken"];
            _logger.LogInformation($"Session refresh token: {sessionRefreshToken}");
            return;
        }

        private async Task SendVerifyEmail(AppUser user, string mode)
        {
            string token = string.Empty;
            string verifyPage = string.Empty;
            string verifyUrl = string.Empty;
            string verifyUrlFirstParagraph = string.Empty;
            string verifyUrlEndParagraph = string.Empty;
            string message = string.Empty;
            string subject = string.Empty;

            //TODO: Not working, need to fix.
            // string origin = Request.Headers["origin"];
            string origin = _config.GetSection("ClientAppSettings:Origin").Get<string>() ?? string.Empty;
            

            switch (mode)
            {
                case "register":
                    token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    verifyPage = "verifyEmail";
                    verifyUrlFirstParagraph = "<p>Please click the below link to verify your email address:</p><p><a href='";
                    verifyUrlEndParagraph = "'>Click to verify email</a></p>";
                    subject = "Confirm Email Address";
                    break;
                case "forgotPassword":
                    token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    verifyPage = "resetPassword";
                    verifyUrlFirstParagraph = "<p>Please click the below link to reset your password:</p><p><a href='";
                    verifyUrlEndParagraph = "'>Click to reset password</a></p>";
                    subject = "Reset Password Email";
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogInformation($"Token is null.");
                return;
            }

            _logger.LogInformation($"raw token: {token}");
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            _logger.LogInformation($"encrypt token: {token}");

            verifyUrl = $"{origin}/account/{verifyPage}?token={token}&email={user.Email}";
            message = $"{verifyUrlFirstParagraph}{verifyUrl}{verifyUrlEndParagraph}";

            _logger.LogInformation($"mode: {mode}");
            _logger.LogInformation($"user.UserName: {user.UserName}");
            _logger.LogInformation($"user.Email: {user.Email}");
            _logger.LogInformation($"origin: {origin}");
            _logger.LogInformation($"message: {message}");
            _logger.LogInformation($"token: {token}");
            _logger.LogInformation($"verifyUrl: {verifyUrl}");
            await _emailSender.SendEmailAsync(user.Email, subject, message);
        }
    }
}