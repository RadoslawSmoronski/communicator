using Application.DTOs;
using Application.Interfaces;
using MediatR;
using Shared.Result;

namespace Application.Auth.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoggedUserDto>>
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public LoginUserCommandHandler(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task<Result<LoggedUserDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var loggedUserResult = await _userService.LoginAsync(request.Email, request.Password);
            if (!loggedUserResult.IsSuccess)
                return loggedUserResult.Error!;

            var accessToken = await _tokenService.CreateAccessTokenAsync(loggedUserResult.Value.Id);
            if (!accessToken.IsSuccess)
                return accessToken.Error!;

            var refreshToken = await _tokenService.CreateRefreshTokenAsync(loggedUserResult.Value.Id);
            if (!refreshToken.IsSuccess)
                return refreshToken.Error!;

            loggedUserResult.Value.AccessToken = accessToken.Value;
            loggedUserResult.Value.RefreshToken = refreshToken.Value;

            return loggedUserResult.Value;
        }
    }
}
