using ChatCommunicator.Contracts;

namespace ChatCommunicator.Managers.Interfaces
{
    public interface IUsersConnectionManager
    {
        Task AddUpdateAsync(string connectionId, Guid userId);
        Task RemoveAsync(string connectionId, Guid userId);

        List<Guid> GetOnlineUsersIdAsync();
        Task<bool> IsUserOnlineAsync(Guid userId);
        List<string>? GetUserConnectionsId(Guid UserId);
    }
}
