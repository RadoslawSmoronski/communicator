using MediatR;
using Shared.Result;

namespace Application.Users.Queries.GetUsers
{
    public record GetUsersQuery(string Search, Guid? CanBeInvitedByUserId) : IRequest<Result<List<GetUsersReadModel>>>;
}
