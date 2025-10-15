using Domain.Entities;

namespace Application.Repositories
{
    public interface IFriendshipInvitationRepository : IBaseRepository<FriendshipInvitation>
    {
        Task<IReadOnlyList<FriendshipInvitation>> GetAllAsync(Guid userId);
        Task<FriendshipInvitation?> GetById(Guid id);
        Task<bool> IsExistAsync(Guid user1Id, Guid user2Id);
    }
}
