using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Commands.DeclineInvitation;

public record DeclineInvitationCommand(Guid InvitationId) : IRequest<Result>, IRequireInvitationParticipant
{
    public Guid? FriendInvitationId => InvitationId;
};
