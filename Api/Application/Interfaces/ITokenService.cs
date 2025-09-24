using Application.DTOs;
using Shared.Result;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        //Access Token
        Task<Result<string>> CreateAccessTokenAsync(Guid userId);
        Task<Result<RefreshAccessTokenResponseDto>> RefreshAccessTokenAsync(Guid refreshToken);

        //RefreshToken
        Task<Result<Guid>> CreateRefreshTokenAsync(Guid userId);
    }
}
