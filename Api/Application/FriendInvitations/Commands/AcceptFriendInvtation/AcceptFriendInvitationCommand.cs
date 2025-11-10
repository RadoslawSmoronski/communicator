using Application.Common.Security;
using Application.FriendInvitations.Commands.AcceptFriendInvtation;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.AcceptFriendInvtation
{
    public record AcceptFriendInvitationCommand(Guid InvitationId) : IRequest<Result<AcceptFriendInvitationReadModel>>, IRequireFriendInvitationRecipient
    {
        public Guid FriendInvitationId => InvitationId;
    }
}
