using Domain;
using Application.RefreshTokens;
using Application.Core;
using API.DTOs;
using Application.Module;

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
    
        public async Task<TokenDto> CreateToken(AppUser user, bool reset = false)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            string secretKey = _config.GetSection("JWTSettings:TokenKey").Value ?? string.Empty;
            int requestAccessTokenExpiresTime = _config.GetSection("JWTSettings:AccessTokenExpirationTime").Get<int?>() ?? -1;
            int requestRefreshTokenExpiresTime = _config.GetSection("JWTSettings:RefreshTokenExpirationTime").Get<int?>() ?? -1;

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("Secret key is not set.");
            }
            if (requestAccessTokenExpiresTime == -1)
            {
                throw new Exception("Access token expires time is not set.");
            }
            if (requestRefreshTokenExpiresTime == -1)
            {
                throw new Exception("Refresh token expires time is not set.");
            }

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            DateTime accessTokenExpiresTime = DateTime.Now.AddMinutes(requestAccessTokenExpiresTime);
            //TODO: For test
            // DateTime accessTokenExpiresTime = DateTime.Now.AddSeconds(requestAccessTokenExpiresTime);

            string accessTokenExpiresUnixTime = DateTimeTool.ConvertToUnixTime(accessTokenExpiresTime);
        
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = accessTokenExpiresTime,
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            string refreshToken = string.Empty;
            DateTime refreshTokenExpiresTime = DateTime.Now.AddDays(requestRefreshTokenExpiresTime);;
            //TODO: For test
            // DateTime refreshTokenExpiresTime = DateTime.Now.AddMinutes(requestRefreshTokenExpiresTime);;

            if (reset)
            {
                Result<List<RefreshTokenDto>>? allRefreshTokenDtoFromDb = await _mediator.Send(
                    new List.Query {
                        AppUserId = user.Id,
                        Predicate = "all"
                    }
                );

                List<RefreshTokenDto> refreshTokenDtoList = allRefreshTokenDtoFromDb.Value ?? new List<RefreshTokenDto>();

                if (refreshTokenDtoList.Count > 0)
                {
                    Result<RefreshTokenDto>? refreshTokenDtoFromDb = await _mediator.Send(
                        new Retrieve.Query {
                            AppUserId = user.Id,
                            Predicate = ""
                        }
                    );
                    RefreshTokenDto? refreshTokenDto = refreshTokenDtoFromDb.Value ?? new RefreshTokenDto();

                    if (!string.IsNullOrEmpty(refreshTokenDto.Token))
                    {
                        bool IsExpired = ValidateTokenEffective(refreshTokenDto.ExpirationTime);
                        if (IsExpired)
                        {
                            await _mediator.Send(new Revoke.Command { AppUserId = user.Id });
                            refreshToken = GenerateRefreshToken();
                            await SaveRefreshTokenToDB(user.Id, token.Id, refreshToken, refreshTokenExpiresTime);
                        }
                        else
                        {
                            refreshToken = refreshTokenDto.Token;
                        }
                    }
                }
                else
                {
                    refreshToken = GenerateRefreshToken();
                    await SaveRefreshTokenToDB(user.Id, token.Id, refreshToken, refreshTokenExpiresTime);
                }
            }

            _logger.LogInformation($"Refresh token created for user {user.UserName}, JwtId: {token.Id}, RefreshToken: {refreshToken}.");

            return new TokenDto
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken,
                ExpiresIn = accessTokenExpiresUnixTime
            };
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

        public async Task SaveRefreshTokenToDB(
            string userId, string jwtId, string refreshToken, DateTime refreshTokenExpiresTime
        )
        {
            RefreshToken generatedRefreshToken = new RefreshToken
            {
                AppUserId = userId,
                Token = refreshToken,
                JwtId = jwtId,
                IsUsed = false,
                IsRevoked = false,
                ExpirationTime = refreshTokenExpiresTime
            };
            await _mediator.Send(new Create.Command { RefreshToken = generatedRefreshToken });
        }

        public bool ValidateTokenEffective(DateTime expiresTime)
        {
            int bothOfCompareResult = DateTimeTool.CompareBothOfTime(DateTime.Now, expiresTime);
            bool IsExpired = false;

            if (bothOfCompareResult > 0)
            {
                IsExpired = true;
            }
            return IsExpired;
        }

        public bool VerifyToken(string token)
        {
            string secretKey = _config.GetSection("JWTSettings:TokenKey").Get<string>() ?? string.Empty;
            
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new Exception("Secret key is not set.");
            }

            var validationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey)
                ),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken = null;

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch(SecurityTokenException)
            {
                return false; 
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
            return validatedToken != null;
        }
    }
}