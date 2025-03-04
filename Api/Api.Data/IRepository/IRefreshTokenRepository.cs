using Api.Models;

namespace Api.Data.IRepository
{
    public interface IRefreshTokenRepository
    {
        Task<bool> IsTokenValidAsync(string token);
        Task SaveTokenAsync(RefreshToken refreshToken);
        Task DeleteTokenAsync(string token);
        Task<string?> GetRefreshTokenByUserIdAsync(string userId);
        Task<string?> GetUserIdByRefreshTokenAsync(string refreshToken);
        Task<int> RemoveExpiredRefreshTokensAsync();
    }
}
