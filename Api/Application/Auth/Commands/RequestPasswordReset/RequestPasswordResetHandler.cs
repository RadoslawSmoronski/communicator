using Application.Interfaces;
using Application.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Result;
using System.Net;

namespace Application.Auth.Commands.RequestPasswordReset
{
    public class RequestPasswordResetHandler : IRequestHandler<RequestPasswordResetCommand, Result<string>>
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly RecoveryPasswordMessageSettings _recoveryPasswordMessageSettings;
        private readonly ILogger<RequestPasswordResetHandler> _logger;

        public RequestPasswordResetHandler(
            IUserService userService,
            IEmailService emailService,
            IOptions<RecoveryPasswordMessageSettings> options,
            ILogger<RequestPasswordResetHandler> logger)
        {
            _userService = userService;
            _emailService = emailService;
            _recoveryPasswordMessageSettings = options.Value;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
        {
            var passwordToken = await _userService.GeneratePasswordResetTokenAsync(request.Email);
            var encodedToken = WebUtility.UrlEncode(passwordToken.Value.Token);

            var content = CreateEmailContent(encodedToken, passwordToken.Value.UserId);
            var emailResult = await _emailService.SendAsync(request.Email, _recoveryPasswordMessageSettings.Title, content);

            if (emailResult.IsSuccess)
            {
                _logger.LogInformation("Password reset email sent to: {Email}", request.Email);
                return encodedToken; //refactor: to delete after clean architecture refactor
            }

            _logger.LogError("Failed to send password reset email to: {Email}. Error: {Error}", request.Email, emailResult.Error?.Description);
            return Result<string>.Failure(emailResult.Error ?? Error.Failure("EmailSend", "Failed to send password reset email."));
        }

        private string CreateEmailContent(string token, Guid userId)
        {
            var address = $"{_recoveryPasswordMessageSettings.Address}?userId={userId}&token={token}";
            return _recoveryPasswordMessageSettings.Content.Replace("[address]", address);
        }
    }
}
