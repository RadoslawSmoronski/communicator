using Domain.Entities;

namespace Application.Repositories
{
    public interface IFriendshipRepository : IBaseRepository<Friendship>
    {
        Task<List<Friendship>> GetAllAsync(Guid userId);
        Task<Friendship?> Get(Guid friendshipId);
        Task<bool> IsExistAsync(Guid user1Id, Guid user2Id);
    }
}
