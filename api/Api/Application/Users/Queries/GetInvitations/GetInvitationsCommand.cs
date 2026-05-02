using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.Users.Queries.GetInvitations
{
    public record GetInvitationsCommand(Guid UserId) : IRequest<Result<List<GetInvitationsReadModel>>>, IRequireSameUser
    {
        public Guid TargetUserId => UserId;
    }
}
