using Application.DTOs;
using Application.Interfaces;
using Application.Repositories;
using Application.Settings;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Result;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly JWTTokenSettings _accessTokenSettings;
        private readonly RefreshTokenSettings _refreshTokenSettings;
        private readonly ILogger<TokenService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public TokenService(UserManager<UserAccount> userManager,
            IOptions<JWTTokenSettings> accessTokenOptions,
            IOptions<RefreshTokenSettings> refreshTokenOptions,
            ILogger<TokenService> logger,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _accessTokenSettings = accessTokenOptions.Value;
            _refreshTokenSettings = refreshTokenOptions.Value;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

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

        public async Task<Result<RefreshAccessTokenResponseDto>> RefreshAccessTokenAsync(Guid refreshToken)
        {
            _logger.LogInformation("[TokenService - RefreshAccessTokenAsync] Attempting to refresh access token with refresh token: {RefreshToken}", refreshToken);

            try
            {
                var refreshTokenFromDb = await GetRefreshTokenAsync(refreshToken);

                if (refreshTokenFromDb == null)
                {
                    _logger.LogWarning("[TokenService - RefreshAccessTokenAsync] Refresh token not found or expired: {RefreshToken}", refreshToken);
                    return Error.Unauthorized("RefreshTokenUnauthorized", $"Refresh token '{refreshToken}' was not found or is expired.");
                }

                var user = await _userManager.FindByIdAsync(refreshTokenFromDb.UserId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("[TokenService - RefreshAccessTokenAsync] User not found for refresh token: {RefreshToken}, userId: {UserId}", refreshToken, refreshTokenFromDb.UserId);
                    return Error.NotFound("UserNotFound", $"User with id '{refreshTokenFromDb.UserId}' was not found.");
                }

                _logger.LogInformation("[TokenService - RefreshAccessTokenAsync] User found. Creating new access token for user: {UserName}, userId: {UserId}", user.UserName, user.Id);

                var accessToken = CreateJwtToken(user);
                var newRefreshToken = Guid.NewGuid();

                refreshTokenFromDb.Token = newRefreshToken;
                refreshTokenFromDb.Expiration = DateTime.UtcNow; // refactor: Consider renaming 'Expiration' to 'CreatedAt' if this is creation time.

                _unitOfWork.RefreshTokens.Update(refreshTokenFromDb);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("[TokenService - RefreshAccessTokenAsync] Access token and refresh token refreshed successfully for userId: {UserId}", user.Id);

                return new RefreshAccessTokenResponseDto()
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[TokenService - RefreshAccessTokenAsync] Exception occurred while refreshing access token with refresh token: {RefreshToken}", refreshToken);
                return Error.Failure("RefreshTokenCreationFailed", "An error occurred while creating the refresh token.");
            }
        }

        private async Task<RefreshToken?> GetRefreshTokenAsync(Guid refreshToken)
        {
            var expirationTimeSpan = TimeSpan.FromSeconds(_refreshTokenSettings.RefreshTokenLifeInSeconds);
            var expirationTime = DateTime.UtcNow - expirationTimeSpan;

            return await _unitOfWork.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.Expiration > expirationTime);
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
