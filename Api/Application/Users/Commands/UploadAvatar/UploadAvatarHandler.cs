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
            => await _userAvatarService.UploadAvatarAsync(request.UserId, request.File);
    }
}
