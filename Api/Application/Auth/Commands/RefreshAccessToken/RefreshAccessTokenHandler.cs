using Application.Interfaces;
using MediatR;
using Shared.Result;

namespace Application.Auth.Commands.RefreshAccessToken
{
    public class RefreshAccessTokenHandler : IRequestHandler<RefreshAccessTokenCommand, Result<RefreshAccessTokenReadModel>>
    {
        private readonly ITokenService _tokenService;

        public RefreshAccessTokenHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<Result<RefreshAccessTokenReadModel>> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshTokenResult = await _tokenService.GetRefreshTokenAsync(request.RefreshToken);
            if (!refreshTokenResult.IsSuccess)
                return refreshTokenResult.Error!;

            var refreshToken = refreshTokenResult.Value;

            var updateRefreshToken = await _tokenService.UpdateRefreshToken(refreshToken);
            if(!updateRefreshToken.IsSuccess)
                return updateRefreshToken.Error!;

            var updatedRefreshToken = updateRefreshToken.Value;

            var accessTokenResult = await _tokenService.CreateAccessTokenAsync(updatedRefreshToken.UserId);
            if(!accessTokenResult.IsSuccess)
                return accessTokenResult.Error!;

            var accessToken = accessTokenResult.Value;

            return new RefreshAccessTokenReadModel(
                AccessToken: accessToken,
                RefreshToken: refreshToken.Token
                );
        }
    }
}
