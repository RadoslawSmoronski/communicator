using Domain.Entities;

namespace Application.Repositories
{
    public interface IFriendshipRepository
    {
        Task<List<Friendship>> GetAllAsync(Guid userId);
        Task<Friendship?> GetAsync(Guid friendshipId);
        Task<bool> IsExistAsync(Guid user1Id, Guid user2Id);
        Task AddAsync(Friendship entity);
        Task DeleteAsync(Guid id);
        void Update(Friendship entity);
    }
}
