using Application.Common.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.UploadAvatar
{
    public class UploadAvatarHandler(IUserAvatarService userAvatarService) : IRequestHandler<UploadAvatarCommand, Result<string>>
    {
        private readonly IUserAvatarService _userAvatarService = userAvatarService;

        public async Task<Result<string>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
            => await _userAvatarService.UploadAvatarAsync(request.UserId, request.File);
    }
}
