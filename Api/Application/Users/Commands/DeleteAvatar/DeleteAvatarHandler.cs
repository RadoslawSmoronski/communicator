using Application.Common.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.DeleteAvatar
{
    public class DeleteAvatarHandler : IRequestHandler<DeleteAvatarCommand, Result>
    {
        private readonly IUserService _userService;
        private readonly IUserAvatarService _userAvatarService;

        public DeleteAvatarHandler(IUserService userService, IUserAvatarService userAvatarService)
        {
            _userService = userService;
            _userAvatarService = userAvatarService;
        }

        public async Task<Result> Handle(DeleteAvatarCommand request, CancellationToken cancellationToken)
            => await _userAvatarService.DeleteAvatarAsync(request.UserId);
    }
}
