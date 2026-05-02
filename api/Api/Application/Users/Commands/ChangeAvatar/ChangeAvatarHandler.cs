using Application.Common.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.ChangeAvatar
{
    public class ChangeAvatarHandler(IUserAvatarService userAvatarService) : IRequestHandler<ChangeAvatarCommand, Result<string>>
    {
        private readonly IUserAvatarService _userAvatarService = userAvatarService;

        public async Task<Result<string>> Handle(ChangeAvatarCommand request, CancellationToken cancellationToken)
                => await _userAvatarService.ChangeAvatarAsync(request.UserId, request.File);

    }
}
