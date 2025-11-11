using Application.Common.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.DeleteAvatar
{
    public class DeleteAvatarHandler(IUserAvatarService userAvatarService) : IRequestHandler<DeleteAvatarCommand, Result>
    {
        private readonly IUserAvatarService _userAvatarService = userAvatarService;

        public async Task<Result> Handle(DeleteAvatarCommand request, CancellationToken cancellationToken)
            => await _userAvatarService.DeleteAvatarAsync(request.UserId);
    }
}
