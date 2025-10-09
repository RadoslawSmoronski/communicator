using Shared.Result;

namespace Application.Interfaces
{
    public interface IFriendshipService
    {
        Task<Result<Guid>> AddAsync(Guid user1, Guid user2);
        //Task<ResultT<List<FriendDto>>> GetFriendsAsync(Guid userId);
        //Task<bool> IsFriendsExistAsync(Guid userId1, Guid userId2);
        //Task<ResultT<List<string>>> GetUserOnlineFriendsConnectionsIdAsync(Guid userId);
        //Task<Result> DeleteAsync(Guid friendshipId);
    }
}
