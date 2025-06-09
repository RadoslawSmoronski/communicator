using ChatCommunicator.Shared.Result;

namespace ChatCommunicator.Services.Interfaces
{
    public interface IChatFriendsService
    {
        Task<Result> AddFriendAndCreateConversationAsync(Guid user1Id, Guid user2Id);
    }
}
