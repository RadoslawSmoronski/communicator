using ChatCommunicator.Infrastructure.UnitOfWork;
using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos.Service;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using ChatCommunicator.Application.Services.Interfaces;

namespace ChatCommunicator.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly TimeSpan _accesTokenLifeTime = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _refreshTokenLifeTime = TimeSpan.FromDays(7);
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public TokenService(IConfiguration config,
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
            if (inputUser == null || string.IsNullOrEmpty(inputUser.UserName) || inputUser.Id == Guid.Empty)
            {
                return Error.Validation("USER_INPUT_INVALID", "User, username, and user ID are required and cannot be null or empty.");
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
                return Error.Unknown("TOKEN_GENERATION_FAILED", "An unexpected error occurred while generating the access token.");
            }
        }

        public async Task<ResultT<RefreshAccessTokenDto>> RefreshAccessTokenAsync(Guid refreshToken)
        {
            if (!await IsRefreshTokenValidAsync(refreshToken))
            {
                return Error.Unauthorized("REFRESH_TOKEN_INVALID", "The provided refresh token is invalid or has expired.");
            }

            var refreshTokenResult = await GetRefreshTokenObjectAsync(refreshToken);


            if (refreshTokenResult == null || refreshTokenResult.UserId == Guid.Empty)
            {
                return Error.Unknown("REFRESH_TOKEN_DATA_INCONSISTENCY", "The refresh token exists, but required user data is missing. This may indicate data inconsistency.");
            }

            var user = await ValidateUserAsync(refreshTokenResult.UserId);
            if (user.Error != null)
            {
                return user.Error;
            }

            var accessToken = CreateJwtToken(user.Value);
            var newRefreshToken = Guid.NewGuid();

            refreshTokenResult.Token = newRefreshToken;
            refreshTokenResult.Expiration = DateTime.UtcNow.Add(_refreshTokenLifeTime);

            try
            {
                _unitOfWork.RefreshTokens.Update(refreshTokenResult);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception)
            {
                return Error.Unknown("TOKEN_REFRESH_PROCESS_FAILED", "An unexpected error occurred during the token refresh process.");
            }

            var result = new RefreshAccessTokenDto()
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };

            return result;
        }

        public async Task<ResultT<Guid>> CreateRefreshTokenAsync(Guid userId)
        {
            try
            {
                var oldRefreshToken = await GetRefreshTokenObjectByUserIdAsync(userId);

                var newRefreshToken = Guid.NewGuid();
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
                return Error.Unknown("REFRESH_TOKEN_SAVE_FAILED", "Failed to save the refresh token due to an unexpected internal error.");
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
                return Error.Failure("NO_EXPIRED_REFRESH_TOKENS", "There are no expired refresh tokens to remove.");
            }
            catch (Exception)
            {
                //Console.WriteLine("[RemoveExpiredRefreshTokensAsync] An internal server error occurred.");
                return Error.Unknown("REFRESH_TOKEN_CLEANUP_FAILED", "An unexpected error occurred while removing expired refresh tokens.");
            }
        }

        private async Task<bool> IsRefreshTokenValidAsync(Guid refreshToken)
        {
            return await _unitOfWork.RefreshTokens
                .AnyAsync(rt => rt.Token == refreshToken && rt.Expiration > DateTime.UtcNow);
        }

        private async Task<RefreshToken?> GetRefreshTokenObjectAsync(Guid refreshToken)
        {
            return await _unitOfWork.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken);
        }

        private async Task<RefreshToken?> GetRefreshTokenObjectByUserIdAsync(Guid userId)
        {
            return await _unitOfWork.RefreshTokens
                .FirstOrDefaultAsync(x => x.UserId == userId);

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
            if (user == null || user.Id == Guid.Empty || user.UserName == null)
            {
                throw new ArgumentNullException(nameof(user), "User, UserId or UserName cannot be null.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
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

        private async Task<ResultT<UserAccount>> ValidateUserAsync(Guid? userId)
        {
            if (userId == Guid.Empty)
            {
                return Error.Validation("USER_ID_MISSING", "User ID is required and cannot be null or empty.");
            }

            string idString = userId?.ToString() ?? string.Empty;

            var user = await _userManager.FindByIdAsync(idString);
            if (user == null || string.IsNullOrEmpty(user.UserName))
            {
                return Error.Unauthorized("INVALID_USER", "Authentication failed. User not found or invalid.");
            }

            return user;
        }
    }
}
