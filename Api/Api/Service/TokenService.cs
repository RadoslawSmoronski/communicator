using Api.Data.IRepository;
using Api.Models;
using Api.Models.Dtos.Controllers.UserController;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly TimeSpan _accesTokenLifeTime = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _refreshTokenLifeTime = TimeSpan.FromDays(7);
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public TokenService(IConfiguration config, IRefreshTokenRepository refreshTokenRepository)
        {
            _config = config;
            var signingKey = _config["JWT:SigningKey"];

            if (signingKey == null)
            {
                throw new ArgumentNullException("JWT:SigningKey", "Signing key must be provided in configuration.");
            }

            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

            _refreshTokenRepository = refreshTokenRepository;
        }

        public string CreateAccessToken(UserAccount user)
        {
            var claims = new List<Claim>
            {
                //new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.Add(_accesTokenLifeTime),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }


        public ClaimsPrincipal ValidateAccessToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["JWT:Issuer"],
                    ValidAudience = _config["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken securityToken);

                return principal;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Invalid token.", ex);
            }
        }

        public async Task<RefreshAccessTokenDto> RefreshAccessToken(string refreshToken, string accessToken)
        {
            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("Refresh token and access token must not be null or empty.");
            }

            if (!await ValidateRefreshToken(refreshToken))
            {
                throw new UnauthorizedAccessException("Failed to validate refresh token.");
            }

            var principal = ValidateAccessToken(accessToken);
            if (principal == null || principal.Identity == null)
            {
                throw new UnauthorizedAccessException("Failed to validate access token or identity.");
            }

            var userUsernameClaim = principal.FindFirst(ClaimTypes.Name);
            if (userUsernameClaim == null || string.IsNullOrEmpty(userUsernameClaim.Value))
            {
                throw new UnauthorizedAccessException("Username claim not found or is empty.");
            }

            var userUsername = userUsernameClaim.Value;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Failed to retrieve user ID from access token.");
            }

            var userId = userIdClaim.Value;

            var newAccessToken = CreateAccessToken(new UserAccount { Id = userId, UserName = userUsername });
            var newRefreshToken = CreateRefreshToken();

            await SaveRefreshTokenAsync(userId, newRefreshToken);

            return new RefreshAccessTokenDto() {
                RefreshToken = newRefreshToken,
                AccessToken = newAccessToken
            };
        }

        public string CreateRefreshToken()
        {
            var randomNumber = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }

        public async Task<bool> ValidateRefreshToken(string refreshToken)
        {
            return await _refreshTokenRepository.IsTokenValidAsync(refreshToken);
        }

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("User ID and refresh token must not be null or empty.");
            }

            try
            {
                var oldRefreshToken = await _refreshTokenRepository.GetRefreshTokenAsyncByUserId(userId);

                if (oldRefreshToken != null)
                {
                    await RemoveRefreshTokenAsync(oldRefreshToken);
                }

                await _refreshTokenRepository.SaveTokenAsync(new RefreshToken
                {
                    Token = refreshToken,
                    UserId = userId,
                    Expiration = DateTime.UtcNow.AddDays(7)
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to save refresh token, please try again later.", ex);
            }
        }

        public async Task RemoveRefreshTokenAsync(string refreshToken)
        {
            await _refreshTokenRepository.DeleteTokenAsync(refreshToken);
        }

    }
}
