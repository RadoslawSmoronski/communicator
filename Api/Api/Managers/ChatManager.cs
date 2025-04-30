using Api.Data.UnitOfWork;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Chat;
using Api.Models.Dtos;
using Api.Models.Dtos.Chat;
using Api.Utilities.Result;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System;

namespace Api.Managers
{
    public class ChatManager : IChatManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IFriendsManager _friendsManager;
        private readonly IMapper _mapper;

        private readonly int _messagesPageSize = 10;

        public ChatManager(IUnitOfWork unitOfWork,
            UserManager<UserAccount> userManager,
            IFriendsManager friendsManager,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _friendsManager = friendsManager;
            _mapper = mapper;
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

        public async Task<Result> DeleteConversationAsync(string conversationId)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                return Error.BadRequest("CONVERSATIONID_IS_EMPTY", "ConversationId cannot be null or empty.");
            }

            try
            {
                var conversation = await GetConversationByIdAsync(conversationId);

                if (conversation != null)
                {
                    await _DeleteConversationAsync(conversation);
                    return Result.Success();
                }

                return Error.NotFound("CONVERSATION_NOT_FOUND", "Conversation was not found.");
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<ChatDto>>> GetChatsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Error.BadRequest("USERID_IS_EMPTY", "UserId cannot be null or empty.");
            }

            try
            {
                var conversations = await _unitOfWork.Conversations.WhereAsync(
                    x => x.User1Id == userId || x.User2Id == userId,
                    x => x.User1,
                    x => x.User2,
                    x => x.LastMessage
                );

                var chatDtos = conversations.Select(x =>
                {
                    var isUser1 = x.User1Id == userId;
                    var friend = isUser1 ? x.User2 : x.User1;

                    if (friend?.UserName == null)
                        throw new Exception("Friend's username is null");

                    return new ChatDto
                    {
                        FriendId = friend.Id,
                        FriendUserName = friend.UserName,
                        ConversationId = x.Id.ToString(),
                        LastMessageId = x.LastMessageId?.ToString(),
                        LastMessageContent = x.LastMessage?.Content,
                        IsFriendSenderMessage = x.LastMessage?.SenderId == friend.Id,
                        LastMessageTimestamp = x.LastMessage?.Timestamp
                    };
                }).ToList();

                return chatDtos;
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<MessageDto>>> GetPagedMessagesFromMessageIdAsync(string conversationId, string fromMessageId)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                return Error.BadRequest("CONVERSATIONID_IS_EMPTY", "ConversationId cannot be null or empty.");
            }

            try
            {
                if (await GetConversationByIdAsync(conversationId) == null)
                {
                    return Error.NotFound("CONVERSATION_ID_NOT_FOUND", "ConversationId was not found.");
                }

                var messages = await _GetPagedMessagesFromMessageIdAsync(conversationId, fromMessageId);

                var messageDtos = _mapper.Map<List<MessageDto>>(messages);

                return messageDtos;
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<Result> SaveMessageAsync(Message message)
        {
            try
            {
                await _SaveMessageAsync(message);
                await UpdateLastMessageInConversationAsync(message);
                return Result.Success();
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        private async Task<List<Message>> _GetPagedMessagesFromMessageIdAsync(string conversationId, string fromMessageId)
        {
            if (!Guid.TryParse(conversationId, out var conversationIdGuid))
                throw new ArgumentException("Invalid conversationId", nameof(conversationId));

            if (!Guid.TryParse(fromMessageId, out var fromMessageIdGuid))
                throw new ArgumentException("Invalid fromMessageId", nameof(fromMessageId));

            var messages = await _unitOfWork.Messages.GetPagedMessagesFromMessageIdAsync(
                conversationIdGuid,
                fromMessageIdGuid,
                _messagesPageSize
                );

            return messages.ToList();
        }

        private async Task _SaveMessageAsync(Message message)
        {
            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveAsync();
        }

        private async Task UpdateLastMessageInConversationAsync(Message message)
        {
            var conversation = message.Conversation;
            conversation.LastMessage = message;
            conversation.LastMessageTime = message.Timestamp;
            conversation.LastMessageId = message.Id;

            _unitOfWork.Conversations.Update(conversation);
            await _unitOfWork.SaveAsync();
        }

        private async Task<Conversation?> GetConversationAsync(string user1Id, string user2Id)
        {
            return await _unitOfWork.Conversations.FirstOrDefaultAsync(x =>
                (x.User1Id == user1Id && x.User2Id == user2Id) ||
                (x.User1Id == user2Id && x.User2Id == user1Id));
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

        private async Task _DeleteConversationAsync(Conversation conversation)
        {
            _unitOfWork.Conversations.Delete(conversation);
            await _unitOfWork.SaveAsync();
        }

    }
}
