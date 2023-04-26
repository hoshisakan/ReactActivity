using API.DTOs;
using API.Services;
using Domain;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
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
                UserDto? userDto = await CreateUserObject(user);

                string userSetTokenCacheKey = "user";

                //TODO: Set cache, but don't expire it.
                // await _cache.SetStringAsync(userSetTokenCacheKey, userDto.Token);

                //TODO: Set cache and add expiration time.
                await _cache.SetStringAsync(
                    "user", userDto.Token,
                    new DistributedCacheEntryOptions{
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                        // AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
                    }
                );

                string decryptToken = await _cache.GetStringAsync(userSetTokenCacheKey) ?? "Empty";
                _logger.LogInformation($"username: {user.UserName}, _cache: {decryptToken}");

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

        [HttpGet]
        private async Task<UserDto> CreateUserObject(AppUser user)
        {
            TokenDto token = await _tokenService.CreateToken(user);
            return new UserDto
            {
                DisplayName = user.DisplayName,
                Image = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                Token = token.AccessToken,
                RefreshToken = token.RefreshToken,
                Username = user.UserName
            };
        }
    }
}