using Application.Common.Security;
using Application.DTOs;
using MediatR;
using Shared.Result;

namespace Application.Users.Queries.GetInvitations
{
    public record GetInvitationsCommand(Guid UserId) : IRequest<Result<List<FriendshipInvitationDto>>>, IRequireSameUser
    {
        public Guid TargetUserId => UserId;
    }
}
