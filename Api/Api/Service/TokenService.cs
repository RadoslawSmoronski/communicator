using Api.Data.IRepository;
using Api.Models;
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

        public string CreateToken(UserAccount user)
        {
            var claims = new List<Claim>
            {
                //new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.UserName)
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

        public string CreateRefreshToken()
        {
            var randomNumber = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["JWT:SigningKey"]);

            try
            {
                var prinipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _config["JWT:Issuer"],
                    ValidAudience = _config["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken securityToken);

                return prinipal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ValidateRefreshToken(string refreshToken)
        {
            return await _refreshTokenRepository.IsTokenValidAsync(refreshToken);
        }

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken)
        {
            try
            {
                var oldRefreshToken = await _refreshTokenRepository.GetRefreshTokenAsyncByUserId(userId);

                if(oldRefreshToken != null)
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
            catch(Exception ex) 
            {
                Console.WriteLine($"Error saving refresh token: {ex.Message}");
                throw new Exception("Failed to save refresh token, please try again later.", ex);
            }
        }

        public async Task RemoveRefreshTokenAsync(string refreshToken)
        {
            await _refreshTokenRepository.DeleteTokenAsync(refreshToken);
        }

    }
}
