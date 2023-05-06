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
using Microsoft.Extensions.Caching.Distributed;


namespace API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;

        private readonly ILogger<TokenService> _logger;
        private readonly IMediator _mediator;
        private readonly TokenValidationParameters _tokenValidationParams;

        public TokenService(IConfiguration config, IDistributedCache cache,
            IMediator mediator, ILogger<TokenService> logger,
            TokenValidationParameters tokenValidationParams
        )
        {
            _config = config;
            _cache = cache;
            _mediator = mediator;
            _logger = logger;
            _tokenValidationParams = tokenValidationParams;
        }

        public string CreateToken(AppUser user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            string secretKey = _config.GetSection("JWTSettings:TokenKey").Value ?? string.Empty;
            string issuer = _config.GetSection("JWTSettings:Issuer").Get<string>() ?? string.Empty;
            string audience = _config.GetSection("JWTSettings:Audience").Get<string>() ?? string.Empty;
            int requestAccessTokenExpiresTime = _config.GetSection("JWTSettings:AccessTokenExpiresTime").Get<int?>() ?? -1;

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer)
                || string.IsNullOrEmpty(audience)
            )
            {
                throw new Exception("Secret key or issuer or audience is not set.");
            }
            if (requestAccessTokenExpiresTime == -1)
            {
                throw new Exception("Access token expires time is not set.");
            }

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            DateTime accessTokenExpiresTime = DateTime.Now.AddMinutes(requestAccessTokenExpiresTime);
            //TODO: For test
            // DateTime accessTokenExpiresTime = DateTime.Now.AddSeconds(requestAccessTokenExpiresTime);

            string accessTokenExpiresUnixTime = DateTimeTool.ConvertToUnixTime(accessTokenExpiresTime);
        
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = accessTokenExpiresTime,
                SigningCredentials = creds,
                Issuer = issuer,
                Audience = audience
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return new RefreshToken{
                    Token = Convert.ToBase64String(randomNumber) + Guid.NewGuid()
                };
            }
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
            bool validatedTokenResult = false;

            try {
                JwtSecurityTokenHandler? tokenHandler = new JwtSecurityTokenHandler();
                ClaimsPrincipal? tokenInVerification = tokenHandler.ValidateToken(token, _tokenValidationParams, out SecurityToken securityToken);
                validatedTokenResult = tokenInVerification != null;

                _logger.LogInformation($"Validate token result: {validatedTokenResult}");

                if (validatedTokenResult)
                {
                    string expiryDateTime = tokenInVerification.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Exp).Select(c => c.Value).SingleOrDefault();
                    string jWTId = tokenInVerification.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Jti).Select(c => c.Value).SingleOrDefault();
                    _logger.LogInformation(
                        $"ExpiryDateTime: {expiryDateTime}, JWTId: {jWTId}"
                    );
                }
            }
            catch(SecurityTokenException se)
            {
                _logger.LogError(se.Message);
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
            }
            return validatedTokenResult;
        }

        public ValidateTokenDto RecoveryToken(string token)
        {
            bool validatedTokenResult = false;
            ValidateTokenDto validateTokenDto = new ValidateTokenDto();

            try {
                JwtSecurityTokenHandler? tokenHandler = new JwtSecurityTokenHandler();
                TokenValidationParameters _noLifeTimeValidationTokenParameters = GetNoLifeTimeValidateTokenParameters();
                ClaimsPrincipal? tokenInVerification = tokenHandler.ValidateToken(token, _noLifeTimeValidationTokenParameters, out SecurityToken securityToken);
                validatedTokenResult = tokenInVerification != null;

                _logger.LogInformation($"Validate token result: {validatedTokenResult}");

                if (validatedTokenResult)
                {
                    validateTokenDto.JwtId = tokenInVerification.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Jti).Select(c => c.Value).SingleOrDefault();
                    string expiryUnixTimeStamp = tokenInVerification.Claims.Where(c => c.Type == JwtRegisteredClaimNames.Exp).Select(c => c.Value).SingleOrDefault();
                    
                    if (!string.IsNullOrEmpty(expiryUnixTimeStamp))
                    {
                        DateTime expiryDateTime = DateTimeTool.UnixTimeStampToDateTime(expiryUnixTimeStamp);
                        int compareBothOfResult = DateTimeTool.CompareBothOfTime(DateTime.Now, expiryDateTime);
                        if (compareBothOfResult > 0)
                        {
                            validateTokenDto.IsExpired = true;
                        }
                        else
                        {
                            validateTokenDto.IsExpired = false;
                        }
                        _logger.LogInformation($"expiryDateTime: {expiryDateTime}");
                        _logger.LogInformation($"expiryUnixTimeStamp: {expiryUnixTimeStamp}");
                    }
                }
                validateTokenDto.IsValid = validatedTokenResult;
            }
            catch(SecurityTokenException se)
            {
                _logger.LogError(se.Message);
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
            }
            return validateTokenDto;
        }

        public TokenValidationParameters GetNoLifeTimeValidateTokenParameters()
        {
            string secretKey = _config.GetSection("JWTSettings:TokenKey").Get<string>() ?? string.Empty;
            string issuer = _config.GetSection("JWTSettings:Issuer").Get<string>() ?? string.Empty;
            string audience = _config.GetSection("JWTSettings:Audience").Get<string>() ?? string.Empty;
            
            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer)
                || string.IsNullOrEmpty(audience)
            )
            {
                throw new Exception("Secret key or issuer or audience is not set.");
            }
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey)
                ),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}