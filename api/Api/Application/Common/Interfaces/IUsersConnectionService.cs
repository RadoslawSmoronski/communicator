namespace Application.Common.Interfaces
{
    public interface IUsersConnectionService
    {
        Task AddUpdateAsync(string connectionId, Guid userId);
        Task RemoveAsync(string connectionId, Guid userId);
        Task<List<Guid>> GetOnlineUsersIdAsync();
        Task<bool> IsUserOnlineAsync(Guid userId);
        List<string>? GetUserConnectionsId(Guid UserId);
    }
}
