using Application.Interfaces;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.DeleteAvatar
{
    public class DeleteAvatarHandler : IRequestHandler<DeleteAvatarCommand, Result>
    {
        private readonly IUserService _userService;

        public DeleteAvatarHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result> Handle(DeleteAvatarCommand request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.UserId))
            {
                return Error.Unauthorized("Unauthorized", "User is not authorized.");
            }

            return await _userService.DeleteAvatarAsync(request.UserId);
        }
    }
}
