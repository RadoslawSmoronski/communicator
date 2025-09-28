using Application.DTOs;
using Application.Interfaces;
using Application.Settings;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Result;
using System.Net;

namespace Application.Users.Commands.RegisterUser
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<RegisteredDto>>
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly ILogger<RegisterUserHandler> _logger;

        private readonly ConfirmEmailMessageSettings _confirmEmailMessageSettings;

        public RegisterUserHandler(IUserService userService,
            IEmailService emailService,
            ILogger<RegisterUserHandler> logger,
            IOptions<ConfirmEmailMessageSettings> confirmEmailMessageOptions)
        {
            _userService = userService;
            _emailService = emailService;
            _logger = logger;
            _confirmEmailMessageSettings = confirmEmailMessageOptions.Value;
        }

        public async Task<Result<RegisteredDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var registerResult = await _userService.RegisterAsync(request.Email, request.Username, request.Password);

            if (!registerResult.IsSuccess)
            {
                _logger.LogWarning("User registration failed for email {Email}: {Error}", request.Email, registerResult.Error?.Description);
                return registerResult.Error ?? Error.Failure("RegisterUser", "Unknown registration error");
            }

            var registredUser = registerResult.Value;
            var tokenResult = await _userService.GenerateEmailConfirmationTokenAsync(registredUser.Id);

            if (!tokenResult.IsSuccess)
            {
                _logger.LogError("Failed to generate email confirmation token for userId {UserId}: {Error}", registredUser.Id, tokenResult.Error?.Description);
                return tokenResult.Error ?? Error.Failure("EmailToken", "Unknown token generation error");
            }

            var encodedToken = WebUtility.UrlEncode(tokenResult.Value);
            registredUser.ConfirmToken = encodedToken;

            var emailContent = CreateEmailContent(registredUser.Id, registredUser.ConfirmToken);

            var emailResult = await _emailService.SendAsync(registredUser.Email, _confirmEmailMessageSettings.Title, emailContent);

            if (emailResult.IsSuccess)
            {
                _logger.LogInformation("User registered and confirmation email sent to {Email}", registredUser.Email);
                return registredUser;
            }

            _logger.LogError("Failed to send confirmation email to {Email}: {Error}", registredUser.Email, emailResult.Error?.Description);
            return emailResult.Error ?? Error.Failure("EmailSend", "Unknown email sending error");
        }

        private string CreateEmailContent(Guid userId, string token)
        {
            var address = $"{_confirmEmailMessageSettings.Address}userId={userId.ToString()}&token={token}";
            return _confirmEmailMessageSettings.Content.Replace("[address]", address);
        }
    }
}
