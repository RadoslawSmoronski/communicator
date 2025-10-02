using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.DecelineInvitation
{
    public record DecelineInvitationCommand(Guid InvitationId) : IRequest<Result>;
}
