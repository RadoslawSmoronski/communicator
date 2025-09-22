using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Services.Interfaces;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace ChatCommunicator.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        private readonly RecoveryPasswordMessageSettings _recoveryPasswordMessageSettings;

        public AuthService(
            UserManager<UserAccount> userManager,
            IEmailService emailService,
            IOptions<RecoveryPasswordMessageSettings> recoveryPasswordMessageSettings,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
            _recoveryPasswordMessageSettings = recoveryPasswordMessageSettings.Value;
        }

        public async Task<ResultT<string>> SendPasswordResetEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                return Error.NotFound("USER_NOT_FOUND", "User with the given email does not exist.");
            }

            try
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebUtility.UrlEncode(token);

                var content = CreateEmailContent(encodedToken, user.Id);

                var emailResult = await _emailService.SendAsync(email, _recoveryPasswordMessageSettings.Title, content);
                if (!emailResult.IsSuccess)
                {
                    _logger.LogError("Failed to send password reset email to {Email}.", email);
                    return emailResult.Error!;
                }

                _logger.LogInformation("Password reset token sent to {Email}.", email);
                return encodedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate password reset token for {Email}.", email);
                return Error.Unknown("TOKEN_GENERATION_FAILED", "Failed to generate password reset token.");
            }
        }
        private string CreateEmailContent(string token, Guid userId)
        {
            var address = $"{_recoveryPasswordMessageSettings.Address}/?userId={userId}&token={token}";
            return _recoveryPasswordMessageSettings.Content.Replace("[address]", address);
        }

        public async Task<ResultT<string>> ResetPasswordAsync(Guid userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                _logger.LogWarning("Password reset attempted for null user.");
                return Error.NotFound("USER_NOT_FOUND", "User does not exist.");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Password reset attempted with empty token for user {UserId}.", user.Id);
                return Error.Validation("TOKEN_EMPTY", "Password reset token is required.");
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                _logger.LogWarning("Password reset attempted with empty new password for user {UserId}.", user.Id);
                return Error.Validation("PASSWORD_EMPTY", "New password is required.");
            }

            var decodedToken = WebUtility.UrlDecode(token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset successful for user {UserId}.", user.Id);
                return newPassword;
            }

            var errorDescription = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogError("Password reset failed for user {UserId}: {Errors}", user.Id, errorDescription);
            return Error.Failure("PASSWORD_RESET_FAILED", errorDescription);
        }
    }
}
