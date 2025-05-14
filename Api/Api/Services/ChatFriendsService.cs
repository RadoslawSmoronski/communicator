using Api.Managers.Interfaces;
using Api.Services.Interfaces;
using Api.Utilities.Result;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Api.Services
{
    public class ChatFriendsService : IChatFriendsService
    {
        IChatManager _chatManager;
        IFriendsManager _friendManager;

        public ChatFriendsService(IChatManager chatManager, IFriendsManager friendManager)
        {
            _chatManager = chatManager;
            _friendManager = friendManager;
        }

        public async Task<Result> AddFriendAndCreateConversationAsync(Guid user1Id, Guid user2Id)
        {
            if (user1Id == Guid.Empty || user2Id == Guid.Empty)
            {
                return Error.Validation("USERID_IS_EMPTY", "UserId or FriendId cannot be empty.");
            }

            var resultFriends = await _friendManager.AddFriendsAsync(user1Id, user2Id);

            if (resultFriends.IsSuccess == false && resultFriends.Error != null)
            {
                return resultFriends.Error;
            }

            var resultConversation = await _chatManager.GetOrCreateConversationAsync(user1Id, user2Id);

            if (resultConversation.IsSuccess == false && resultConversation.Error != null)
            {
                return resultConversation.Error;
            }

            if(resultConversation.IsSuccess && resultConversation.IsSuccess)
            {
                return Result.Success();
            }

            return Error.Unknown("INTERNAL_SERVER_ERROR", "Problem with AddFriendAndCreateConversationAsync, conntact with administrator.");
        }
    }
}
