using Application.Interfaces.Users;
using Application.Users.Commands.UploadAvatar;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Application.Users.Commands.ChangeAvatar
{
    public class ChangeAvatarHandler : IRequestHandler<ChangeAvatarCommand, Result<string>>
    {
        private readonly ILogger<ChangeAvatarCommand> _logger;
        private readonly IUserAvatarService _userAvatarService;
        private readonly IUserService _userService;

        public ChangeAvatarHandler(ILogger<ChangeAvatarCommand> logger, IUserAvatarService userAvatarService, IUserService userService)
        {
            _logger = logger;
            _userAvatarService = userAvatarService;
            _userService = userService;
        }

        public async Task<Result<string>> Handle(ChangeAvatarCommand request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.UserId))
            {
                _logger.LogWarning("Unauthorized attempt to change avatar for user {UserId}", request.UserId);
                return Error.Unauthorized("Unauthorized", "User is not authorized.");
            }

            var result = await _userAvatarService.ChangeAvatarAsync(request.UserId, request.File);

            if (result.IsSuccess)
            {
                return Result<string>.Success(result.Value);
            }

            _logger.LogError("Failed to change avatar for user {UserId}: {Error}", request.UserId, result.Error?.Description);
            return result.Error ?? Error.Unknown("UnknownError", "An unknown error occurred.");
        }

    }
}
