using Application.Common.Interfaces;
using Application.Interfaces.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Result;
using System.Numerics;

namespace Application.Users.Commands.ChangeUsername
{
    public class ChangeUsernameHandler : IRequestHandler<ChangeUsernameCommand, Result<string>>
    {
        private readonly IUserService _userService;
        private readonly ILogger<ChangeUsernameCommand> _logger;

        public ChangeUsernameHandler(IUserService userService, ILogger<ChangeUsernameCommand> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(ChangeUsernameCommand request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.UserId))
            {
                _logger.LogWarning("Unauthorized attempt to change username for user {UserId}", request.UserId);
                return Error.Unauthorized("Unauthorized", "User is not authorized.");
            }

            var result = await _userService.ChangeUsernameAsync(request.UserId, request.NewPassword);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Username changed successfully for user {UserId}", request.UserId);
                return result.Value;
            }

            _logger.LogError("Failed to change username for user {UserId}: {Error}", request.UserId, result.Error?.Description);
            return result.Error ?? Error.Unknown("UnknownError", "An unknown error occurred.");
        }
    }
}
