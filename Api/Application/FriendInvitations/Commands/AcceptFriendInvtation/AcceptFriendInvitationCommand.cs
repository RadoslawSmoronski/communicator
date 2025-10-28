using Application.DTOs;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Commands.AcceptFriendInvtation
{
    public record AcceptFriendInvitationCommand(Guid InvitationId) : IRequest<Result<AcceptFriendshipInviteDto>>;
}
