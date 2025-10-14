using Application.DTOs;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IFriendInvitationsService
    {
        Task<Result<Guid>> SendAsync(Guid senderId, Guid recipientId);
        Task<Result<FriendshipInviteOperationDto>> DeleteAsync(Guid InvitationId);
        Task<Result<FriendshipInviteOperationDto>> AcceptAsync(Guid InvitationId);
        Task<Result<List<FriendshipInvitationDto>>> GetAsync(Guid userId);
        // Task<Result<List<UserToInviteDto>>> GetUsersToInviteByTextAsync(Guid userId, string text); // I need to finish friends service first
    }
}
