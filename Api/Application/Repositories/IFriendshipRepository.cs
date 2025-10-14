using Domain.Entities;

namespace Application.Repositories
{
    public interface IFriendshipRepository
    {
        Task AddAsync(Friendship friendship);
        Task<List<Friendship>> GetAllAsync(Guid userId);
        Task<Friendship?> Get(Guid friendshipId);
        Task<bool> IsExistAsync(Guid user1Id, Guid user2Id);
        Task DeleteAsync(Guid friedshipId);
    }
}
