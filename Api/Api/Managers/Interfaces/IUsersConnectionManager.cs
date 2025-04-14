using Api.Models;

namespace Api.Managers.Interfaces
{
    public interface IUsersConnectionManager
    {
        Task AddUpdateAsync(string connectionId, string userId);
        Task RemoveAsync(string connectionId, string userId);

        List<string> GetOnlineUsersIdAsync();
        Task<bool> IsUserOnlineAsync(string userId);
        List<string>? GetUserConnectionsId(string UserId);
    }
}
