using Application.DTOs;
using Application.Interfaces;
using MediatR;
using Shared.Result;

namespace Application.Auth.Commands.RefreshAccessToken
{
    public class RefreshAccessTokenHandler : IRequestHandler<RefreshAccessTokenCommand, Result<RefreshAccessTokenResponseDto>>
    {
        private readonly ITokenService _tokenService;

        public RefreshAccessTokenHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }


        public async Task<Result<RefreshAccessTokenResponseDto>> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
        {
            var refreshAccessTokenResult = await _tokenService.RefreshAccessTokenAsync(request.refreshToken);
            if (!refreshAccessTokenResult.IsSuccess)
                return refreshAccessTokenResult.Error!;

            return refreshAccessTokenResult.Value;
        }
    }
}
