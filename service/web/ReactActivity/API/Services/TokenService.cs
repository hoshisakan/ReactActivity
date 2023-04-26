using Domain;
using Application.RefreshTokens;
using Application.Core;
using API.DTOs;

using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using MediatR;

namespace API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<TokenService> _logger;
        private readonly IMediator _mediator;

        public TokenService(IConfiguration config, IMediator mediator, ILogger<TokenService> logger)
        {
            _config = config;
            _mediator = mediator;
            _logger = logger;
        }
        
        public string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber) + Guid.NewGuid();
                // return $"{Convert.ToBase64String(randomNumber)}{Guid.NewGuid()}";
            }
        }

        public async Task<TokenDto> CreateToken(AppUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            string secretKey = _config.GetSection("JWTSettings:TokenKey").Value ?? string.Empty;
            int accessTokenExpirationTime = _config.GetSection("JWTSettings:AccessTokenExpirationTime").Get<int?>() ?? -1;
            int refreshTokenExpirationTime = _config.GetSection("JWTSettings:RefreshTokenExpirationTime").Get<int?>() ?? -1;

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("Secret key is not set.");
            }
            if (accessTokenExpirationTime == -1)
            {
                throw new Exception("Access token expires time is not set.");
            }
            if (refreshTokenExpirationTime == -1)
            {
                throw new Exception("Refresh token expires time is not set.");
            }

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                // Expires = DateTime.Now.AddMinutes(accessTokenExpirationTime),
                Expires = DateTime.Now.AddSeconds(accessTokenExpirationTime),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            RefreshToken refreshToken = new RefreshToken
            {
                AppUserId = user.Id,
                Token = GenerateRefreshToken(),
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                ExpirationTime = DateTime.Now.AddDays(refreshTokenExpirationTime)
            };

            // Result<Unit>? result = await _mediator.Send(new Create.Command { RefreshToken = refreshToken });

            _logger.LogInformation($"Refresh token created for user {user.UserName}, JwtId: {refreshToken.JwtId}, RefreshToken: {refreshToken.Token}.");

            return new TokenDto
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken.Token,
            };
        }
    }
}