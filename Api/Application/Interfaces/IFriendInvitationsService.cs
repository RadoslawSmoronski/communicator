using Application.DTOs;
using Domain.Entities;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IFriendInvitationsService
    {
        Task<Result<Guid>> SendAsync(Guid senderId, Guid recipientId);
        Task<Result<FriendshipInviteOperationDto>> DeleteAsync(Guid InvitationId);
        Task<Result<FriendshipInviteOperationDto>> AcceptAsync(Guid InvitationId);
        Task<Result<List<FriendshipInvitationDto>>> GetInvitationsSendedToUserAsync(Guid userId); //refactor
        Task<Result<List<FriendshipInvitation>>> GetUserInvitations(Guid userId);
    }
}
