using Api.Data.UnitOfWork;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Chat;
using Api.Models.Dtos;
using Api.Models.Dtos.Chat;
using Api.Utilities.Result;
using Microsoft.AspNetCore.Identity;

namespace Api.Managers
{
    public class ChatManager : IChatManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IFriendsManager _friendsManager;

        public ChatManager(IUnitOfWork unitOfWork, UserManager<UserAccount> userManager, IFriendsManager friendsManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _friendsManager = friendsManager;   
        }

        public async Task<ResultT<Conversation>> GetOrCreateConversationAsync(string userId, string friendId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(friendId))
            {
                return Error.BadRequest("USERID_IS_EMPTY", "UserId or FriendId cannot be null or empty.");
            }

            if (userId == friendId)
            {
                return Error.BadRequest("USERID_AND_FRIENDID_ARE_THE_SAME", "UserId and FriendId must be different.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null || user.UserName == null)
                {
                    return Error.NotFound("USER_NOT_FOUND", "User was not found.");
                }

                var friendUser = await _userManager.FindByIdAsync(friendId);

                if (friendUser == null || friendUser.UserName == null)
                {
                    return Error.NotFound("FRIENDUSER_NOT_FOUND", "Friend was not found.");
                }

                var isFriends = await _friendsManager.IsFriendsExistAsync(userId, friendId);

                if (isFriends == false)
                {
                    return Error.Conflict("USERS_ARE_NOT_FRIENDS", "Users are not friends.");
                }

                var conversation = await GetConversationAsync(userId, friendId);

                if (conversation == null)
                {
                    conversation = await CreateConversationAsync(user, friendUser);
                }

                return conversation;
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<Result> DeleteConversationAsync(string conversationId) // Need tests
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                return Error.BadRequest("CONVERSATIONID_IS_EMPTY", "ConversationId cannot be null or empty.");
            }

            try
            {
                await _DeleteConversationAsync(conversationId);
                return Result.Success();
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<ChatDto>>> GetChatsAsync(string userId) // Need tests
        {
            var conversations = await _unitOfWork.Conversations.WhereAsync(
                x => (x.User1Id == userId || x.User2Id == userId),
                x => x.User1,
                x => x.User2,
                x => x.LastMessage
                );


            return conversations.Select(x => new ChatDto()
            {
                FriendId = x.User1Id == userId ? x.User2Id : x.User1Id,
                FriendUserName = x.User1.UserName != null && x.User2.UserName != null
                ? (x.User1Id == userId ? x.User2.UserName : x.User1.UserName)
                : throw new Exception(),
                ConversationId = x.Id.ToString(),
                LastMessageId = x.LastMessageId?.ToString(),  
                LastMessageContent = x.LastMessage?.Content,  
                IsFriendSenderMessage = x.LastMessage != null && x.LastMessage.SenderId == (x.User1Id == userId ? x.User2Id : x.User1Id),
                LastMessageTimestamp = x.LastMessage?.Timestamp,
            }
            ).ToList();
        }

        public async Task<ResultT<List<MessageDto>>> GetMessagesAsync(string conversationId) // Need tests
        {
            var messages = await _unitOfWork.Messages.WhereAsync(x => x.ConversationId.ToString() == conversationId);

            if(messages.Any())
            {
                return messages.Select(x => new MessageDto()
                {
                    MessageId = x.Id.ToString(),
                    ConversationId = x.ConversationId.ToString(),
                    SenderId = x.SenderId.ToString(),
                    Content = x.Content,
                    Timestamp = x.Timestamp,
                    IsRead = x.IsRead,
                }).ToList();
            }

            return Error.NotFound("MESSAGES_NOT_FOUND", "Messages were not found.");
        }

        public async Task<Result> SaveMessageAsync(Message message) // Need tests
        {
            try
            {
                await _SaveMessageAsync(message);
                return Result.Success();
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        private async Task _SaveMessageAsync(Message message)
        {
            _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveAsync();
        }

        private async Task<Conversation?> GetConversationAsync(string user1Id, string user2Id)
        {
            return await _unitOfWork.Conversations.FirstOrDefaultAsync(x =>
                (x.User1Id == user1Id || x.User2Id == user2Id) ||
                (x.User1Id == user2Id || x.User2Id == user1Id));
        }

        private async Task<Conversation?> GetConversationByIdAsync(string conversationId)
        {
            return await _unitOfWork.Conversations.FirstOrDefaultAsync(x => x.Id == Guid.Parse(conversationId));
        }

        private async Task<Conversation> CreateConversationAsync(UserAccount user1, UserAccount user2)
        {
            var conversation = new Conversation()
            {
                User1Id = user1.Id,
                User2Id = user2.Id,
                User1 = user1,
                User2 = user2
            };

            await _unitOfWork.Conversations.AddAsync(conversation);
            await _unitOfWork.SaveAsync();

            return conversation;
        }

        private async Task _DeleteConversationAsync(string conversationId)
        {
            var conversation = await GetConversationByIdAsync(conversationId);

            _unitOfWork.Conversations.Delete(conversation!);
            await _unitOfWork.SaveAsync();
        }

    }
}
