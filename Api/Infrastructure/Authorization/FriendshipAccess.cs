using Application.Common.Authorization;
using Application.Repositories;

namespace Infrastructure.Authorization
{
    public class FriendshipAccess : IFriendshipAccess
    {
        private readonly IUnitOfWork _unitOfWork;

        public FriendshipAccess(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> IsParticipantAsync(Guid userId, Guid friendshipId, CancellationToken ct)
        {
            var friendship = await _unitOfWork.Friendships.GetAsync(friendshipId);
            return friendship is not null && (friendship.User1Id == userId || friendship.User2Id == userId);

        }
    }
}
