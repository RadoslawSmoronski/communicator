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

                var content = CreateEmailContent(encodedToken);

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
        private string CreateEmailContent(string token)
        {
            var address = $"{_recoveryPasswordMessageSettings.Address}/?token={token}";
            return _recoveryPasswordMessageSettings.Content.Replace("[address]", address);
        }
    }
}
