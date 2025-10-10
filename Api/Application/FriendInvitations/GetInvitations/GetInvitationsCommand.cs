using Application.DTOs;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.GetInvitations
{
    public record GetInvitationsCommand(Guid UserId) : IRequest<Result<List<FriendshipInvitationDto>>>;
}
