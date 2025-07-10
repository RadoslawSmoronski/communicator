using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Shared.Result;
using Microsoft.Extensions.Logging;

namespace ChatCommunicator.Application.Services
{
    public class ChatFriendsService : IChatFriendsService
    {
        IChatService _chatService;
        IFriendsService _friendsService;
        ILogger<ChatFriendsService> _logger;

        public ChatFriendsService(IChatService chatService, IFriendsService friendService, ILogger<ChatFriendsService> logger)
        {
            _chatService = chatService;
            _friendsService = friendService;
            _logger = logger;
        }

        public async Task<Result> AddFriendAndCreateConversationAsync(Guid user1Id, Guid user2Id)
        {
            if (user1Id == Guid.Empty || user2Id == Guid.Empty)
            {
                _logger.LogWarning("AddFriendAndCreateConversationAsync failed: one or both user IDs are empty. user1Id: {User1Id}, user2Id: {User2Id}", user1Id, user2Id);
                return Error.Validation("USERID_IS_EMPTY", "UserId or FriendId cannot be empty.");
            }

            _logger.LogInformation("Starting AddFriendAndCreateConversationAsync for user1Id: {User1Id}, user2Id: {User2Id}", user1Id, user2Id);

            var resultFriends = await _friendsService.AddFriendsAsync(user1Id, user2Id);

            if (resultFriends.IsSuccess == false && resultFriends.Error != null)
            {
                _logger.LogWarning("AddFriendsAsync failed for user1Id: {User1Id}, user2Id: {User2Id}. Error: {ErrorType} - {ErrorDescription}", user1Id, user2Id, resultFriends.Error.ErrorType, resultFriends.Error.Description);
                return resultFriends.Error;
            }

            _logger.LogInformation("Friends added successfully between user1Id: {User1Id} and user2Id: {User2Id}", user1Id, user2Id);

            var resultConversation = await _chatService.GetOrCreateConversationAsync(user1Id, user2Id);

            if (resultConversation.IsSuccess == false && resultConversation.Error != null)
            {
                _logger.LogWarning("GetOrCreateConversationAsync failed for user1Id: {User1Id}, user2Id: {User2Id}. Error: {ErrorType} - {ErrorDescription}", user1Id, user2Id, resultConversation.Error.ErrorType, resultConversation.Error.Description);
                return resultConversation.Error;
            }

            if(resultConversation.IsSuccess && resultFriends.IsSuccess)
            {
                _logger.LogInformation("Conversation created successfully between user1Id: {User1Id} and user2Id: {User2Id}", user1Id, user2Id);
                return Result.Success();
            }

            _logger.LogError("Unknown error occurred in AddFriendAndCreateConversationAsync for user1Id: {User1Id}, user2Id: {User2Id}", user1Id, user2Id);
            return Error.Unknown("INTERNAL_SERVER_ERROR", "Problem with AddFriendAndCreateConversationAsync, contact with administrator.");
        }
    }
}
