using Application.Interfaces.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Application.Users.Commands.UploadAvatar
{
    public class UploadAvatarHandler : IRequestHandler<UploadAvatarCommand, Result<string>>
    {
        private readonly ILogger<UploadAvatarCommand> _logger;
        private readonly IUserAvatarService _userAvatarService;
        private readonly IUserService _userService;

        public UploadAvatarHandler(ILogger<UploadAvatarCommand> logger, IUserAvatarService userAvatarService, IUserService userService)
        {
            _logger = logger;
            _userAvatarService = userAvatarService;
            _userService = userService;
        }

        public async Task<Result<string>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.UserId))
            {
                _logger.LogWarning("Unauthorized attempt to change username for user {UserId}", request.UserId);
                return Error.Unauthorized("Unauthorized", "User is not authorized.");
            }

            var result = await _userAvatarService.UploadAvatarAsync(request.UserId, request.File);

            if (result.IsSuccess)
            {
                return result.Value;
            }

            _logger.LogError("Failed to change username for user {UserId}: {Error}", request.UserId, result.Error?.Description);
            return result.Error ?? Error.Unknown("UnknownError", "An unknown error occurred.");
        }
    }
}
