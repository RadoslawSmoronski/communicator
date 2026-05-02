using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Commands.AcceptFriendInvtation
{
    public record AcceptFriendInvitationCommand(Guid InvitationId) : IRequest<Result<AcceptFriendInvitationReadModel>>, IRequireFriendInvitationRecipient
    {
        public Guid FriendInvitationId => InvitationId;
    }
}
