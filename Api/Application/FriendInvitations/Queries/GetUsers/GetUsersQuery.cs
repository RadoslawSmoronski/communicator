using Application.DTOs;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Queries.GetUsers
{
    public record GetUsersQuery(string Search, Guid? InvitableFor) : IRequest<Result<List<UserToInviteDto>>>;
}
