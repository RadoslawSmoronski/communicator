using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.Friendships.Commands.DeleteFriendship
{
    public record DeleteFriendshipCommand(Guid FriendshipId) : IRequest<Result>, IRequireFriendshipParticipant
    {
        public Guid FriendshipId => FriendshipId;
    };
}
