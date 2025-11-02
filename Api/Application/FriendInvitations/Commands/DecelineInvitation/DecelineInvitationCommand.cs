using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Commands.DecelineInvitation
{
    public record DecelineInvitationCommand(Guid InvitationId) : IRequest<Result>, IRequireInvitationParticipant
    {
        public Guid? FriendInvitationId => InvitationId;
    };
}
