using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Application.Users.Commands.UploadAvatar
{
    public class UploadAvatarHandler : IRequestHandler<UploadAvatarCommand, Result<string>>
    {
        private readonly IUserService _userService;
        private readonly ILogger<UploadAvatarCommand> _logger;

        public UploadAvatarHandler(IUserService userService, ILogger<UploadAvatarCommand> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public async Task<Result<string>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.UserId))
            {
                _logger.LogWarning("Unauthorized attempt to change username for user {UserId}", request.UserId);
                return Error.Unauthorized("Unauthorized", "User is not authorized.");
            }

            var result = await _userService.UploadAvatarAsync(request.UserId, request.File);

            if (result.IsSuccess)
            {
                return result.Value;
            }

            _logger.LogError("Failed to change username for user {UserId}: {Error}", request.UserId, result.Error?.Description);
            return result.Error ?? Error.Unknown("UnknownError", "An unknown error occurred.");
        }
    }
}
