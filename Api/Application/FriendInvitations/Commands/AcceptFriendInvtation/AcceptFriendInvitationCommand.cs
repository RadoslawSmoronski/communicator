using Application.Common.Security;
using Application.DTOs;
using Application.FriendInvitations.Commands.AcceptFriendInvtation;
using Domain.Entities;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.AcceptFriendInvtation
{
    public record AcceptFriendInvitationCommand(Guid InvitationId) : IRequest<Result<AcceptFriendInvitationReadModel>>, IRequireFriendInvitationRecipient
    {
        public Guid FriendInvitationId => InvitationId;
    }
}
