using Application.DTOs;
using Application.Users.Queries.GetInvitations;
using Domain.Entities;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IFriendInvitationsService
    {
        Task<Result<Guid>> SendAsync(Guid senderId, Guid recipientId);
        Task<Result<FriendshipInviteOperationDto>> DeleteAsync(Guid InvitationId);
        Task<Result<FriendshipInviteOperationDto>> AcceptAsync(Guid InvitationId);
        Task<Result<List<GetInvitationsReadModel>>> GetInvitationsSendedToUserAsync(Guid userId); //refactor
        Task<Result<List<FriendshipInvitation>>> GetUserInvitations(Guid userId);
    }
}
