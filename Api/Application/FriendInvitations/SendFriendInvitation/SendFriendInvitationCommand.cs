using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.SendFriendInvitation
{
    public record SendFriendInvitationCommand(Guid SenderId, Guid RecipientId) : IRequest<Result<Guid>>;
}
