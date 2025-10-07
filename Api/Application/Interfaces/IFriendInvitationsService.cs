using Application.DTOs;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IFriendInvitationsService
    {
        Task<Result<Guid>> SendInviteAsync(Guid senderId, Guid recipientId);
        Task<Result<FriendshipInviteOperationDto>> DeleteInviteAsync(Guid InvitationId);
        Task<Result<FriendshipInviteOperationDto>> AcceptInviteAsync(Guid InvitationId);
        //Task<Result<List<FriendshipInvitationDto>>> GetInvitationsAsync(Guid userId);
        //Task<Result<List<UserToInviteDto>>> GetUsersToInviteByTextAsync(Guid userId, string text);
        //Task<Result> DecelineInviteAsync(Guid senderId, Guid recipientId);
    }
}
