using Application.Common.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.ChangeUsername
{
    public class ChangeUsernameHandler(IUserService userService) : IRequestHandler<ChangeUsernameCommand, Result<string>>
    {
        private readonly IUserService _userService = userService;

        public async Task<Result<string>> Handle(ChangeUsernameCommand request, CancellationToken cancellationToken)
            => await _userService.ChangeUsernameAsync(request.UserId, request.NewPassword);
    }
}
