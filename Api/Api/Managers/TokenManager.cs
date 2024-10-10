using Api.Data.IRepository;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos.Controllers.UserController;
using Api.Utilities.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Service
{
    public class TokenManager : ITokenManager
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly TimeSpan _accesTokenLifeTime = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _refreshTokenLifeTime = TimeSpan.FromDays(7);
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly UserManager<UserAccount> _userManager;

        public TokenManager(IConfiguration config, IRefreshTokenRepository refreshTokenRepository, UserManager<UserAccount> userManager)
        {
            _config = config;
            var signingKey = _config["JWT:SigningKey"];

            if (signingKey == null)
            {
                throw new ArgumentNullException("JWT:SigningKey", "Signing key must be provided in configuration.");
            }

            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

            _refreshTokenRepository = refreshTokenRepository;
            _userManager = userManager;

        }

        public async Task<ResultT<string>> CreateAccessTokenAsync(UserAccount user)
        {

            if(user == null)
            {
                return Error.Validation("USER_IS_NULL", "User must not be null.");
            }

            if(String.IsNullOrEmpty(user.UserName))
            {
                return Error.Validation("USER_USERNAME_IS_NULL", "Username must not be null or empty.");
            }

            if (String.IsNullOrEmpty(user.Id))
            {
                return Error.Validation("USER_ID_IS_NULL", "Username must not be null or empty.");
            }

            try
            {
                var userExist = await _userManager.FindByIdAsync(user.Id);

                if(userExist == null || userExist.UserName != user.UserName)
                {
                    return Error.NotFound("USER_NOT_FOUND", "User doesn't exist.");
                }

                var claims = new List<Claim>
                    {
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
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<string>> RefreshAccessTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Error.Validation("REFRESHTOKEN_IS_NULL", "Refresh token must not be null or empty.");
            }

            if (!await _refreshTokenRepository.IsTokenValidAsync(refreshToken))
            {
                return Error.AccessUnauthorized("REFRESHTOKEN_NOT_FOUND", "Invalid refresh token.");
            }

            var userId = await _refreshTokenRepository.GetUserIdByRefreshTokenAsync(refreshToken);

            if (userId == null)
            {
                return Error.InternalServerError("DATABASE_DATA_ERROR", "Refresh token record doesn't have user data or refresh token have been deleted.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Error.InternalServerError("USER_ERROR", "User form refresh token record doesn't exist.");
            }

            if (String.IsNullOrEmpty(user.UserName))
            {
                return Error.Validation("USER_USERNAME_IS_NULL", "Username must not be null or empty.");
            }

            if (String.IsNullOrEmpty(user.Id))
            {
                return Error.Validation("USER_ID_IS_NULL", "Username must not be null or empty.");
            }

            var claims = new List<Claim>
                    {
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
            var tokenString = tokenHandler.WriteToken(token);

            var newRefreshToken = Guid.NewGuid().ToString();

            await _refreshTokenRepository.SaveTokenAsync(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = userId,
                Expiration = DateTime.UtcNow.AddDays(7)
            });

            return tokenString;
        }

        public async Task<ResultT<string>> CreateRefreshTokenAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Error.Validation("USER_ID_IS_NULL","User ID must not be null or empty.");
            }

            try
            {
                var newRefreshToken = Guid.NewGuid().ToString();

                var oldRefreshToken = await _refreshTokenRepository.GetRefreshTokenAsyncByUserIdAsync(userId);

                if (oldRefreshToken != null)
                {
                    await _refreshTokenRepository.DeleteTokenAsync(oldRefreshToken);
                }

                await _refreshTokenRepository.SaveTokenAsync(new RefreshToken
                {
                    Token = newRefreshToken,
                    UserId = userId,
                    Expiration = DateTime.UtcNow.AddDays(7)
                });

                return newRefreshToken;
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_ERROR", "An internal server error occurred.");
            }
        }
    }
}
