using Domain.Entities;
using Shared.Result;

namespace Application.Interfaces.Users
{
    public interface IFriendInvitationsService
    {
        Task<Result<Guid>> SendInviteAsync(Guid senderId, Guid recipientId);
        Task<Result> DeleteInviteAsync(Guid InviteId);
        //Task<Result<List<FriendshipInvitationDto>>> GetInvitationsAsync(Guid userId);
        //Task<Result<List<UserToInviteDto>>> GetUsersToInviteByTextAsync(Guid userId, string text);
        //Task<Result> DecelineInviteAsync(Guid senderId, Guid recipientId);
    }
}
