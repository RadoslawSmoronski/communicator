using Application.Common.Interfaces;
using Application.Repositories;
using Application.Common.Settings;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Result;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Infrastructure.Entities;

namespace Infrastructure.Services
{
    public class TokenService(
        UserManager<UserAccount> userManager,
        IOptions<JWTTokenSettings> accessTokenOptions,
        IOptions<RefreshTokenSettings> refreshTokenOptions,
        ILogger<TokenService> logger,
        IUnitOfWork unitOfWork
    ) : ITokenService
    {
        private readonly UserManager<UserAccount> _userManager = userManager;
        private readonly JWTTokenSettings _accessTokenSettings = accessTokenOptions.Value;
        private readonly RefreshTokenSettings _refreshTokenSettings = refreshTokenOptions.Value;
        private readonly ILogger<TokenService> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<string>> CreateAccessTokenAsync(Guid userId)
        {
            _logger.LogInformation("[TokenService - CreateAccessTokenAsync] Attempting to create access token for user id: {Id}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user is null)
                {
                    _logger.LogWarning("[TokenService - CreateAccessTokenAsync] User not found for id: {Id}", userId);
                    return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
                }

                _logger.LogInformation("[TokenService - CreateAccessTokenAsync] User found. Creating JWT token for user: {UserName}, id: {Id}", user.UserName, user.Id);

                var accessToken = CreateJwtToken(user);

                _logger.LogInformation("[TokenService - CreateAccessTokenAsync] JWT token created successfully for user id: {Id}", userId);

                return accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TokenService - CreateAccessTokenAsync] Exception occurred while creating access token for user id: {Id}", userId);
                return Error.Failure("AccessTokenCreationFailed", "An error occurred while creating the access token.");
            }
        }

        private string CreateJwtToken(UserAccount user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_accessTokenSettings.SigningKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TimeSpan.FromSeconds(_accessTokenSettings.AccessTokenLifeInSeconds)),
                SigningCredentials = creds,
                Issuer = _accessTokenSettings.Issuer,
                Audience = _accessTokenSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var writtenToken = tokenHandler.WriteToken(token);

            return writtenToken;
        }

        public async Task<Result<RefreshToken>> UpdateRefreshToken(RefreshToken refreshToken)
        {
            _logger.LogInformation("[TokenService - RefreshAccessTokenAsync] Attempting to refresh access token with refresh token: {RefreshToken}", refreshToken);

            try
            {
                var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("[TokenService - RefreshAccessTokenAsync] User not found for refresh token: {RefreshToken}, userId: {UserId}", refreshToken, refreshToken.UserId);
                    return Error.NotFound("UserNotFound", $"User with id '{refreshToken.UserId}' was not found.");
                }

                _logger.LogInformation("[TokenService - RefreshAccessTokenAsync] User found. Creating new access token for user: {UserName}, userId: {UserId}", user.UserName, user.Id);

                var newRefreshToken = Guid.NewGuid();

                refreshToken.Token = newRefreshToken;
                refreshToken.CreatedAt = DateTime.UtcNow; // refactor: Consider renaming 'Expiration' to 'CreatedAt' if this is creation time.

                _unitOfWork.RefreshTokens.Update(refreshToken);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("[TokenService - RefreshAccessTokenAsync] Access token and refresh token refreshed successfully for userId: {UserId}", user.Id);

                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TokenService - RefreshAccessTokenAsync] Exception occurred while refreshing access token with refresh token: {RefreshToken}", refreshToken);
                return Error.Failure("RefreshTokenCreationFailed", "An error occurred while creating the refresh token.");
            }
        }

        public async Task<Result<RefreshToken>> GetRefreshTokenAsync(Guid refreshToken)
        {
            _logger.LogInformation("[TokenService - GetRefreshTokenAsync] Attempting to retrieve refresh token: {RefreshToken}", refreshToken);

            try
            {
                var expirationTimeSpan = TimeSpan.FromSeconds(_refreshTokenSettings.RefreshTokenLifeInSeconds);
                var expirationThreshold = DateTime.UtcNow - expirationTimeSpan;

                _logger.LogDebug("[TokenService - GetRefreshTokenAsync] Calculated expiration threshold: {ExpirationThreshold} (lifetime seconds: {LifeSeconds})",
                    expirationThreshold, _refreshTokenSettings.RefreshTokenLifeInSeconds);

                var result = await _unitOfWork.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.CreatedAt > expirationThreshold);

                if (result is null)
                {
                    _logger.LogWarning("[TokenService - GetRefreshTokenAsync] Refresh token not found or expired: {RefreshToken}", refreshToken);
                    return Error.NotFound("RefreshTokenNotFound", $"Refresh token '{refreshToken}' was not found or has expired.");
                }

                _logger.LogInformation("[TokenService - GetRefreshTokenAsync] Refresh token found for userId: {UserId}", result.UserId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TokenService - GetRefreshTokenAsync] Exception occurred while retrieving refresh token: {RefreshToken}", refreshToken);
                return Error.Failure("RefreshTokenRetrievalFailed", "An unexpected error occurred while retrieving the refresh token.");
            }
        }

        public async Task<Result<Guid>> CreateRefreshTokenAsync(Guid userId)
        {
            _logger.LogInformation("[TokenService - CreateRefreshTokenAsync] Attempting to create refresh token for user id: {Id}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user is null)
                {
                    _logger.LogWarning("[TokenService - CreateRefreshTokenAsync] User not found for id: {Id}", userId);
                    return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
                }

                _logger.LogInformation("[TokenService - CreateRefreshTokenAsync] User found. Checking for existing refresh token for user id: {Id}", userId);

                var oldRefreshToken = await GetRefreshTokenObjectByUserIdAsync(userId);

                var newRefreshToken = Guid.NewGuid();

                if (oldRefreshToken != null)
                {
                    _logger.LogInformation("[TokenService - CreateRefreshTokenAsync] Existing refresh token found. Updating token and expiration for user id: {Id}", userId);
                    oldRefreshToken.Token = newRefreshToken;
                    _unitOfWork.RefreshTokens.Update(oldRefreshToken);
                }
                else
                {
                    _logger.LogInformation("[TokenService - CreateRefreshTokenAsync] No existing refresh token found. Creating new refresh token for user id: {Id}", userId);
                    var newObject = new RefreshToken
                    {
                        Token = newRefreshToken,
                        UserId = userId,
                    };

                    await _unitOfWork.RefreshTokens.AddAsync(newObject);
                }

                _logger.LogInformation("[TokenService - CreateRefreshTokenAsync] Saving changes to refresh token for user id: {Id}", userId);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("[TokenService - CreateRefreshTokenAsync] Refresh token created successfully for user id: {Id}", userId);

                return newRefreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TokenService - CreateRefreshTokenAsync] Exception occurred while creating refresh token for user id: {Id}", userId);
                return Error.Failure("RefreshTokenCreationFailed", "An error occurred while creating the refresh token.");
            }
        }

        private async Task<RefreshToken?> GetRefreshTokenObjectByUserIdAsync(Guid userId) => await _unitOfWork.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId);

    }
}
