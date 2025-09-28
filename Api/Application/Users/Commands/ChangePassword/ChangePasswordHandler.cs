using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Application.Users.Commands.ChangePassword
{
    public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IUserService _userService;
        private readonly ILogger<ChangePasswordCommand> _logger;

        public ChangePasswordHandler(IUserService userService, ILogger<ChangePasswordCommand> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.UserId))
            {
                _logger.LogWarning("Unauthorized attempt to change password for user {UserId}", request.UserId);
                return Error.Unauthorized("Unauthorized", "User is not authorized.");
            }

            var result = await _userService.ChangePasswordAsync(request.UserId, request.OldPassword, request.NewPassword);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Password changed successfully for user {UserId}", request.UserId);
                return Result.Success();
            }

            _logger.LogError("Failed to change password for user {UserId}: {Error}", request.UserId, result.Error?.Description);
            return result.Error ?? Error.Unknown("UnknownError", "An unknown error occurred.");
        }
    }
}
