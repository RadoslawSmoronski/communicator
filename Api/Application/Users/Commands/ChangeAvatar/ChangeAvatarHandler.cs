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
                => await _userAvatarService.ChangeAvatarAsync(request.UserId, request.File);

    }
}
