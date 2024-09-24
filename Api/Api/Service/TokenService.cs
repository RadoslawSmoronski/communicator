﻿using Api.Data.IRepository;
using Api.Models;
using Api.Models.Dtos.Controllers.UserController;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<UserAccount> _userManager;

        public CookieOptions AccessTokenCookieOptions { get; }
        public CookieOptions RefreshTokenCookieOptions { get; }

        public TokenService(IConfiguration config, IRefreshTokenRepository refreshTokenRepository, UserManager<UserAccount> userManager)
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


            AccessTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.Add(_accesTokenLifeTime)
            };

            RefreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.Add(_refreshTokenLifeTime)
            };
        }

        public async Task<string> CreateAccessTokenAsync(UserAccount user)
        {
            return await Task.Run(() =>
            {
                if(user == null || user.UserName == null)
                {
                    throw new ArgumentNullException("User object is empty.");
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
            });
        }


        public async Task<ClaimsPrincipal> ValidateAccessTokenAsync(string token)
        {
            return await Task.Run(() =>
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
            });
        }

        public async Task<string> RefreshAccessTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentNullException("Refresh token must not be null or empty.");
            }

            if (!await ValidateRefreshTokenAsync(refreshToken))
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            var userId = await _refreshTokenRepository.GetUserIdByRefreshTokenAsync(refreshToken);

            if (userId == null)
            {
                throw new Exception("Refresh token not found"); //todo notfound exception
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new Exception("User which belong to this refresh was deleted."); //todo notfound exception
            }


            var newAccessToken = await CreateAccessTokenAsync(new UserAccount { Id = userId, UserName = user.UserName });

            await SaveRefreshTokenAsync(userId, refreshToken);

            return newAccessToken;
        }

        public async Task<string> CreateRefreshTokenAsync()
        {
            return await Task.Run(() =>
                {
                    var randomNumber = new byte[32];

                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                }

                return Convert.ToBase64String(randomNumber);
            });
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken)
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
                var oldRefreshToken = await _refreshTokenRepository.GetRefreshTokenAsyncByUserIdAsync(userId);

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
