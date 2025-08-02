using ChatCommunicator.Contracts.Dtos.Friendships;
using ChatCommunicator.Shared.Result;

namespace ChatCommunicator.Application.Services.Interfaces
{
    public interface IChatFriendsService
    {
        Task<ResultT<AcceptFriendshipInviteDto>> AddFriendAndCreateConversationAsync(Guid user1Id, Guid user2Id);
    }
}
