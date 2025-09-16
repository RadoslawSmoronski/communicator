using Application.Interfaces;
using MediatR;
using Shared.Result;
using System.Drawing;

namespace Application.Auth.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<Guid>>
    {
        private readonly IUserService _userService;

        public LoginUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<Guid>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var userId = await _userService.LoginAsync(request.Email, request.Password);
            var error = userId.Error;

            if (userId.IsSuccess)
            {
                return userId.Value;
            }
            else if(error is not null)
            {
                return error;
            }

            return Error.Unknown("test", "test");
        }
    }
}
