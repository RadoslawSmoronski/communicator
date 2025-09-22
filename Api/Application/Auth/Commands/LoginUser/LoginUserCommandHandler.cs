using Application.DTOs;
using Application.Interfaces;
using MediatR;
using Shared.Result;
using System.Drawing;

namespace Application.Auth.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoggedUserDto>>
    {
        private readonly IUserService _userService;

        public LoginUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<LoggedUserDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var userIdResult = await _userService.LoginAsync(request.Email, request.Password);

            if (userIdResult.IsSuccess)
            {
                return userIdResult.Value;
            }

            if (userIdResult.Error is not null)
            {
                return userIdResult.Error;
            }

            return Error.Unknown("LoginFailed", "An unknown error occurred during login.");
        }
    }
}
