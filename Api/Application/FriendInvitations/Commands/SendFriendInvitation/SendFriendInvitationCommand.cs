using Application.DTOs;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Commands.SendFriendInvitation
{
    public record SendFriendInvitationCommand(Guid SenderId, Guid RecipientId) : IRequest<Result<SendFriendInvitationDto>>;
}
