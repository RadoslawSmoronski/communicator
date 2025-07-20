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
using Microsoft.Extensions.Logging;
using ChatCommunicator.Infrastructure.Models;

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
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration config,
            UserManager<UserAccount> userManager,
            IUnitOfWork unitOfWork,
            ILogger<TokenService> logger)
        {
            _config = config;
            var signingKey = _config["JWT:SigningKey"] ?? throw new ArgumentNullException("JWT:SigningKey", "Signing key must be provided in configuration.");

            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ResultT<string>> CreateAccessTokenAsync(UserAccount? inputUser)
        {
            _logger.LogInformation("CreateAccessTokenAsync called for UserId: {UserId}", inputUser?.Id);

            if (inputUser == null || string.IsNullOrEmpty(inputUser.UserName) || inputUser.Id == Guid.Empty)
            {
                _logger.LogWarning("CreateAccessTokenAsync validation failed: invalid user input.");
                return Error.Validation("USER_INPUT_INVALID", "User, username, and user ID are required and cannot be null or empty.");
            }

            try
            {
                var user = await ValidateUserAsync(inputUser.Id);
                if (user.Error != null)
                {
                    _logger.LogWarning("CreateAccessTokenAsync failed: user validation failed for UserId: {UserId}", inputUser.Id);
                    return user.Error;
                }

                var accessToken = CreateJwtToken(inputUser);
                _logger.LogInformation("Access token created successfully for UserId: {UserId}", inputUser.Id);

                return accessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during CreateAccessTokenAsync for UserId: {UserId}", inputUser.Id);
                return Error.Unknown("TOKEN_GENERATION_FAILED", "An unexpected error occurred while generating the access token.");
            }
        }

        public async Task<ResultT<RefreshAccessTokenDto>> RefreshAccessTokenAsync(Guid refreshToken)
        {
            _logger.LogInformation("RefreshAccessTokenAsync called for RefreshToken: {RefreshToken}", refreshToken);

            if (!await IsRefreshTokenValidAsync(refreshToken))
            {
                _logger.LogWarning("RefreshAccessTokenAsync: invalid or expired refresh token {RefreshToken}", refreshToken);
                return Error.Unauthorized("REFRESH_TOKEN_INVALID", "The provided refresh token is invalid or has expired.");
            }

            var refreshTokenResult = await GetRefreshTokenObjectAsync(refreshToken);

            if (refreshTokenResult == null || refreshTokenResult.UserId == Guid.Empty)
            {
                _logger.LogError("RefreshAccessTokenAsync: data inconsistency detected for refresh token {RefreshToken}", refreshToken);
                return Error.Unknown("REFRESH_TOKEN_DATA_INCONSISTENCY", "The refresh token exists, but required user data is missing. This may indicate data inconsistency.");
            }

            var user = await ValidateUserAsync(refreshTokenResult.UserId);
            if (user.Error != null)
            {
                _logger.LogWarning("RefreshAccessTokenAsync: user validation failed for UserId: {UserId}", refreshTokenResult.UserId);
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
                _logger.LogInformation("RefreshAccessTokenAsync: refresh token updated successfully for UserId: {UserId}", refreshTokenResult.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshAccessTokenAsync: failed to save refresh token for UserId: {UserId}", refreshTokenResult.UserId);
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
            _logger.LogInformation("CreateRefreshTokenAsync called for UserId: {UserId}", userId);

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
                    _logger.LogInformation("CreateRefreshTokenAsync: updated existing refresh token for UserId: {UserId}", userId);
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
                    _logger.LogInformation("CreateRefreshTokenAsync: created new refresh token for UserId: {UserId}", userId);
                }

                await _unitOfWork.SaveAsync();

                return newRefreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateRefreshTokenAsync: failed to save refresh token for UserId: {UserId}", userId);
                return Error.Unknown("REFRESH_TOKEN_SAVE_FAILED", "Failed to save the refresh token due to an unexpected internal error.");
            }
        }

        private async Task<bool> IsRefreshTokenValidAsync(Guid refreshToken)
        {
            _logger.LogDebug("IsRefreshTokenValidAsync checking token validity: {RefreshToken}", refreshToken);
            return await _unitOfWork.RefreshTokens
                .AnyAsync(rt => rt.Token == refreshToken && rt.Expiration > DateTime.UtcNow);
        }

        private async Task<RefreshToken?> GetRefreshTokenObjectAsync(Guid refreshToken)
        {
            _logger.LogDebug("GetRefreshTokenObjectAsync retrieving token: {RefreshToken}", refreshToken);
            return await _unitOfWork.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken);
        }

        private async Task<RefreshToken?> GetRefreshTokenObjectByUserIdAsync(Guid userId)
        {
            _logger.LogDebug("GetRefreshTokenObjectByUserIdAsync retrieving token for UserId: {UserId}", userId);
            return await _unitOfWork.RefreshTokens
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        private string CreateJwtToken(UserAccount user)
        {
            _logger.LogDebug("CreateJwtToken called for UserId: {UserId}", user.Id);

            if (user == null || user.Id == Guid.Empty || user.UserName == null)
            {
                _logger.LogError("CreateJwtToken validation failed: user, user.Id or user.UserName is null");
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
            var writtenToken = tokenHandler.WriteToken(token);

            _logger.LogDebug("CreateJwtToken generated token for UserId: {UserId}", user.Id);

            return writtenToken;
        }

        private async Task<ResultT<UserAccount>> ValidateUserAsync(Guid? userId)
        {
            _logger.LogDebug("ValidateUserAsync called for UserId: {UserId}", userId);

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("ValidateUserAsync validation failed: UserId is empty");
                return Error.Validation("USER_ID_MISSING", "User ID is required and cannot be null or empty.");
            }

            string idString = userId?.ToString() ?? string.Empty;

            var user = await _userManager.FindByIdAsync(idString);
            if (user == null || string.IsNullOrEmpty(user.UserName))
            {
                _logger.LogWarning("ValidateUserAsync failed: user not found or invalid for UserId: {UserId}", userId);
                return Error.Unauthorized("INVALID_USER", "Authentication failed. User not found or invalid.");
            }

            _logger.LogDebug("ValidateUserAsync succeeded for UserId: {UserId}", userId);

            return user;
        }
    }
}
