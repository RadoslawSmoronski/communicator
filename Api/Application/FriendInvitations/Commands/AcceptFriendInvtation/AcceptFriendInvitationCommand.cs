using Application.Common.Security;
using Application.DTOs;
using Domain.Entities;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.AcceptFriendInvtation
{
    public record AcceptFriendInvitationCommand(Guid InvitationId) : IRequest<Result<AcceptFriendshipInviteDto>>, IRequireFriendInvitationRecipient
    {
        public Guid FriendInvitationId => InvitationId;
    }
}
