using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Repositories
{
    public interface IFriendshipInvitationRepository
    {
        Task<IReadOnlyList<FriendshipInvitation>> GetAllAsync(Guid userId);
        Task AddAsync(FriendshipInvitation friendshipInvitation);
        Task<FriendshipInvitation?> GetById(Guid id);
        Task DeleteAsync(Guid invitationId);
        Task<bool> IsExistAsync(Guid user1Id, Guid user2Id);
    }
}
