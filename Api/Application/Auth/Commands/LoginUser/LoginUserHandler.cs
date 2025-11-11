using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Auth.Commands.LoginUser
{
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<LoginUserReadModel>>
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public LoginUserHandler(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task<Result<LoginUserReadModel>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var loggedUserResult = await _userService.LoginAsync(request.Email, request.Password);
            if (!loggedUserResult.IsSuccess)
                return loggedUserResult.Error!;

            var user = loggedUserResult.Value;

            var accessToken = await _tokenService.CreateAccessTokenAsync(loggedUserResult.Value.Id);
            if (!accessToken.IsSuccess)
                return accessToken.Error!;

            var refreshToken = await _tokenService.CreateRefreshTokenAsync(loggedUserResult.Value.Id);
            if (!refreshToken.IsSuccess)
                return refreshToken.Error!;

            return new LoginUserReadModel(
                Id: user.Id,
                UserName: user.UserName,
                AvatarUrl: user.AvatarUrl,
                AccessToken: accessToken.Value,
                RefreshToken: refreshToken.Value
                );
        }
    }
}
