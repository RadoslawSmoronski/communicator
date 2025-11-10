using Domain.Entities;
using Shared.Result;

namespace Application.Interfaces
{
    public interface ITokenService
    {
        Task<Result<string>> CreateAccessTokenAsync(Guid userId);
        Task<Result<RefreshToken>> UpdateRefreshToken(RefreshToken refreshToken);

        Task<Result<Guid>> CreateRefreshTokenAsync(Guid userId);
        Task<Result<RefreshToken>> GetRefreshTokenAsync(Guid refreshToken);
    }
}
