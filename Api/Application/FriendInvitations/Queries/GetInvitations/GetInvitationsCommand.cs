using Application.Common.Security;
using Application.DTOs;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Queries.GetInvitations
{
    public record GetInvitationsCommand(Guid UserId) : IRequest<Result<List<FriendshipInvitationDto>>>, IRequireInvitationParticipant
    {
        public Guid? FriendInvitationId { get; }
    }
}
