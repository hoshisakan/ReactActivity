using API.DTOs;
using API.Services;
using Domain;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly ILogger<AppUser> _logger;

        public AccountController(UserManager<AppUser> userManager, TokenService tokenService, ILogger<AppUser> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            AppUser? user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                _logger.LogInformation("User not found.");
                return Unauthorized();
            }

            bool result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (result)
            {
                return new UserDto
                {
                    DisplayName = user.DisplayName,
                    // Image = string.Empty,
                    Image = null,
                    Token = _tokenService.CreateToken(user),
                    Username = user.UserName
                };
            }
            return Unauthorized();
        }
    }
}