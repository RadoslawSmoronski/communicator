using Shared.Result;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        //Access Token
        Task<Result<string>> CreateAccessTokenAsync(Guid userId);
        //Task<Result<RefreshAccessTokenDto>> RefreshAccessTokenAsync(Guid refreshToken);

        //RefreshToken
        //Task<ResultT<Guid>> CreateRefreshTokenAsync(Guid userId);
    }
}
