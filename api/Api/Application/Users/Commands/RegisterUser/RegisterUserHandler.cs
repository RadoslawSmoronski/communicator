using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using Application.Common.Settings;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Result;
using System.Net;

namespace Application.Users.Commands.RegisterUser
{
    public class RegisterUserHandler(
        IUserService userService,
        IEmailService emailService,
        ILogger<RegisterUserHandler> logger,
        IOptions<ConfirmEmailMessageSettings> confirmEmailMessageOptions,
        IMapper mapper)
        : IRequestHandler<RegisterUserCommand, Result<RegisterUserReadModel>>
    {
        private readonly IUserService _userService = userService;
        private readonly IEmailService _emailService = emailService;
        private readonly ILogger<RegisterUserHandler> _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly ConfirmEmailMessageSettings _confirmEmailMessageSettings = confirmEmailMessageOptions.Value;

        public async Task<Result<RegisterUserReadModel>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var registerResult = await _userService.RegisterAsync(request.Email, request.Username, request.Password);

            if (!registerResult.IsSuccess)
            {
                _logger.LogWarning("User registration failed for email {Email}: {Error}", request.Email, registerResult.Error?.Description);
                return registerResult.Error ?? Error.Failure("RegisterUser", "Unknown registration error");
            }

            var registerUserReadModel = _mapper.Map<RegisterUserReadModel>(registerResult.Value);
            var tokenResult = await _userService.GenerateEmailConfirmationTokenAsync(registerUserReadModel.Id);

            if (!tokenResult.IsSuccess)
            {
                _logger.LogError("Failed to generate email confirmation token for userId {UserId}: {Error}", registerUserReadModel.Id, tokenResult.Error?.Description);
                return tokenResult.Error ?? Error.Failure("EmailToken", "Unknown token generation error");
            }

            var encodedToken = WebUtility.UrlEncode(tokenResult.Value);
            registerUserReadModel = registerUserReadModel with { ConfirmToken = encodedToken };

            var emailContent = CreateEmailContent(registerUserReadModel.Id, registerUserReadModel.ConfirmToken);

            var emailResult = await _emailService.SendAsync(registerUserReadModel.Email, _confirmEmailMessageSettings.Title, emailContent);

            if (emailResult.IsSuccess)
            {
                _logger.LogInformation("User registered and confirmation email sent to {Email}", registerUserReadModel.Email);
                return registerUserReadModel;
            }

            _logger.LogError("Failed to send confirmation email to {Email}: {Error}", registerUserReadModel.Email, emailResult.Error?.Description);
            return emailResult.Error ?? Error.Failure("EmailSend", "Unknown email sending error");
        }

        private string CreateEmailContent(Guid userId, string token)
        {
            var address = _confirmEmailMessageSettings.Address + "?userId=" + userId + "&token=" + token;
            return _confirmEmailMessageSettings.Content.Replace("[address]", address);
        }
    }
}
