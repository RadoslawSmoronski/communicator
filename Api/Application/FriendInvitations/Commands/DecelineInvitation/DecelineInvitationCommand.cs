using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Commands.DecelineInvitation
{
    public record DecelineInvitationCommand(Guid InvitationId) : IRequest<Result>;
}
