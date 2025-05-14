using Api.Utilities.Result;

namespace Api.Services.Interfaces
{
    public interface IChatFriendsService
    {
        Task<Result> AddFriendAndCreateConversationAsync(Guid user1Id, Guid user2Id);
    }
}
