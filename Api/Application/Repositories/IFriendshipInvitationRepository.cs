using Domain.Entities;

namespace Application.Repositories
{
    public interface IFriendshipInvitationRepository
    {
        Task<List<FriendshipInvitation>> GetAllAsync(Guid userId);
        Task<FriendshipInvitation?> GetById(Guid id);
        Task<bool> IsExistAsync(Guid user1Id, Guid user2Id);

        Task AddAsync(FriendshipInvitation entity);
        Task DeleteAsync(Guid id);
        void Update(FriendshipInvitation entity);
    }
}
