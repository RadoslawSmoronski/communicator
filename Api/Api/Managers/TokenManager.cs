using Api.Data.UnitOfWork;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos.Service;
using Api.Utilities.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Managers
{
    public class TokenManager : ITokenManager
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly TimeSpan _accesTokenLifeTime = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _refreshTokenLifeTime = TimeSpan.FromDays(7);
        private readonly UserManager<UserAccount> _userManager;

        private readonly IUnitOfWork _unitOfWork;

        public TokenManager(IConfiguration config,
            UserManager<UserAccount> userManager,
            IUnitOfWork unitOfWork)
        {
            _config = config;
            var signingKey = _config["JWT:SigningKey"] ?? throw new ArgumentNullException("JWT:SigningKey", "Signing key must be provided in configuration.");

            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResultT<string>> CreateAccessTokenAsync(UserAccount? inputUser)
        {
            if (inputUser == null || string.IsNullOrEmpty(inputUser.UserName) || string.IsNullOrEmpty(inputUser.Id))
            {
                return Error.BadRequest("USER_IS_INVALID", "User, Username, or User Id cannot be null or empty.");
            }

            try
            {
                var user = await ValidateUserAsync(inputUser.Id);
                if (user.Error != null)
                {
                    return user.Error;
                }

                var accessToken = CreateJwtToken(inputUser);

                return accessToken;
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<RefreshAccessTokenDto>> RefreshAccessTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Error.BadRequest("REFRESHTOKEN_IS_NULL", "Refresh token cannot be null or empty.");
            }

            if (!await IsRefreshTokenValidAsync(refreshToken))
            {
                return Error.NotFound("REFRESHTOKEN_NOT_FOUND", "Refresh token not found.");
            }

            var refreshTokenResult = await GetRefreshTokenObjectAsync(refreshToken);


            if (refreshTokenResult == null || refreshTokenResult.UserId == null)
            {
                return Error.NotFound("DATABASE_DATA_ERROR", "Refresh token record doesn't have user data, or the refresh token has been deleted.");
            }

            var user = await ValidateUserAsync(refreshTokenResult.UserId);
            if (user.Error != null)
            {
                return user.Error;
            }

            var accessToken = CreateJwtToken(user.Value);
            var newRefreshToken = Guid.NewGuid().ToString();

            refreshTokenResult.Token = newRefreshToken;
            refreshTokenResult.Expiration = DateTime.UtcNow.Add(_refreshTokenLifeTime);

            _unitOfWork.RefreshTokens.Update(refreshTokenResult);
            await _unitOfWork.SaveAsync();

            var result = new RefreshAccessTokenDto()
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };

            return result;
        }

        public async Task<ResultT<string>> CreateRefreshTokenAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Error.BadRequest("USER_ID_IS_NULL", "User ID cannot be null or empty.");
            }

            try
            {
                var oldRefreshToken = await GetRefreshTokenObjectByUserIdAsync(userId);

                var newRefreshToken = Guid.NewGuid().ToString();
                var expiration = DateTime.UtcNow.Add(_refreshTokenLifeTime);


                if (oldRefreshToken != null)
                {
                    oldRefreshToken.Token = newRefreshToken;
                    oldRefreshToken.Expiration = expiration;
                    _unitOfWork.RefreshTokens.Update(oldRefreshToken);
                }
                else
                {
                    var newObject = new RefreshToken
                    {
                        Token = newRefreshToken,
                        UserId = userId,
                        Expiration = expiration
                    };

                    await _unitOfWork.RefreshTokens.AddAsync(newObject);
                }

                await _unitOfWork.SaveAsync();

                return newRefreshToken;
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<int>> RemoveExpiredRefreshTokensAsync()
        {
            try
            {
                var removedExpiredTokens = await RemoveFromDbExpiredRefreshTokensAsync();

                if (removedExpiredTokens > 0)
                {
                    //Console.WriteLine($"[RemoveExpiredRefreshTokensAsync] {removedExpiredTokens} expired tokens removed.");
                    return removedExpiredTokens;
                }

                //Console.WriteLine("[RemoveExpiredRefreshTokensAsync] No expired tokens found.");
                return Error.NotFound("EXPIRED_REFRESH_TOKENS_NOT_FOUND", "Expired refresh tokens not found.");
            }
            catch (Exception)
            {
                //Console.WriteLine("[RemoveExpiredRefreshTokensAsync] An internal server error occurred.");
                return Error.InternalServerError("INTERNAL_ERROR", "An internal server error occurred.");
            }
        }

        private async Task<bool> IsRefreshTokenValidAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            return await _unitOfWork.RefreshTokens
                .AnyAsync(rt => rt.Token == refreshToken && rt.Expiration > DateTime.UtcNow);
        }

        private async Task<RefreshToken?> GetRefreshTokenObjectAsync(string refreshToken)
        {
            var result = await _unitOfWork.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);

            return result;
        }

        private async Task<RefreshToken?> GetRefreshTokenObjectByUserIdAsync(string userId)
        {
            var result = await _unitOfWork.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId);

            return result;
        }

        private async Task<int> RemoveFromDbExpiredRefreshTokensAsync()
        {
            var result = await _unitOfWork.RefreshTokens.WhereAsync(x => x.Expiration < DateTime.UtcNow);

            if (result.Any())
            {
                foreach (var refreshToken in result)
                {
                    _unitOfWork.RefreshTokens.Delete(refreshToken);
                }
            }

            return result.Count();
        }

        private string CreateJwtToken(UserAccount user)
        {
            if (user == null || user.Id == null || user.UserName == null)
            {
                throw new ArgumentNullException(nameof(user), "User, UserId or UserName cannot be null.");
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
                Expires = DateTime.UtcNow.Add(_accesTokenLifeTime),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<ResultT<UserAccount>> ValidateUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Error.BadRequest("USER_ID_IS_NULL", "User ID cannot be null or empty.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.UserName))
            {
                return Error.NotFound("USER_NOT_FOUND", "User not found or username is invalid.");
            }

            return user;
        }
    }
}
