using Application.Common.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.ChangePassword
{
    public class ChangePasswordHandler(IUserService userService) : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IUserService _userService = userService;

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
            => await _userService.ChangePasswordAsync(request.UserId, request.OldPassword, request.NewPassword);
    }
}
