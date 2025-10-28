using Application.DTOs;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Queries.GetInvitations
{
    public record GetInvitationsCommand(Guid UserId) : IRequest<Result<List<FriendshipInvitationDto>>>;
}
