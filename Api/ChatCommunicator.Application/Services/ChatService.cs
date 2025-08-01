using ChatCommunicator.Infrastructure.UnitOfWork;
using ChatCommunicator.Contracts.Dtos.Chat;
using ChatCommunicator.Shared.Result;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using ChatCommunicator.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Models.Chat;

namespace ChatCommunicator.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<ChatService> _logger;

        private readonly int _messagesPageSize = 10;

        public ChatService(IUnitOfWork unitOfWork,
            UserManager<UserAccount> userManager,
            IMapper mapper,
            ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResultT<Conversation>> GetOrCreateConversationAsync(Guid userId, Guid friendId)
        {
            if (userId == Guid.Empty || friendId == Guid.Empty)
            {
                _logger.LogWarning("GetOrCreateConversationAsync called with empty userId or friendId. userId: {UserId}, friendId: {FriendId}", userId, friendId);
                return Error.Validation("USERID_IS_EMPTY", "UserId or FriendId cannot be empty.");
            }

            if (userId == friendId)
            {
                _logger.LogWarning("GetOrCreateConversationAsync called with same userId and friendId: {UserId}", userId);
                return Error.Validation("USERID_AND_FRIENDID_ARE_THE_SAME", "UserId and FriendId must be different.");
            }

            try
            {
                _logger.LogInformation("Retrieving user with id {UserId}", userId);
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.UserName == null)
                {
                    _logger.LogWarning("User with id {UserId} not found", userId);
                    return Error.NotFound("USER_NOT_FOUND", "User was not found.");
                }

                _logger.LogInformation("Retrieving friend user with id {FriendId}", friendId);
                var friendUser = await _userManager.FindByIdAsync(friendId.ToString());

                if (friendUser == null || friendUser.UserName == null)
                {
                    _logger.LogWarning("Friend user with id {FriendId} not found", friendId);
                    return Error.NotFound("FRIENDUSER_NOT_FOUND", "Friend was not found.");
                }

                _logger.LogInformation("Looking for existing conversation between user {UserId} and friend {FriendId}", userId, friendId);
                var conversation = await GetConversationAsync(userId, friendId);

                if (conversation == null)
                {
                    _logger.LogInformation("No existing conversation found. Creating new conversation.");
                    conversation = await CreateConversationAsync(user, friendUser);
                    _logger.LogInformation("New conversation created with id {ConversationId}", conversation.Id);
                }
                else
                {
                    _logger.LogInformation("Found existing conversation with id {ConversationId}", conversation.Id);
                }

                return conversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in GetOrCreateConversationAsync for userId: {UserId}, friendId: {FriendId}", userId, friendId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<Result> DeleteConversationAsync(Guid conversationId)
        {
            if (conversationId == Guid.Empty)
            {
                _logger.LogWarning("DeleteConversationAsync called with empty conversationId");
                return Error.Validation("CONVERSATIONID_IS_EMPTY", "ConversationId cannot be empty.");
            }

            try
            {
                _logger.LogInformation("Retrieving conversation with id {ConversationId} for deletion", conversationId);
                var conversation = await GetConversationByIdAsync(conversationId);

                if (conversation != null)
                {
                    _logger.LogInformation("Deleting conversation with id {ConversationId}", conversationId);
                    await _DeleteConversationAsync(conversation);
                    return Result.Success();
                }

                _logger.LogWarning("Conversation with id {ConversationId} not found for deletion", conversationId);
                return Error.NotFound("CONVERSATION_NOT_FOUND", "Conversation was not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in DeleteConversationAsync for conversationId: {ConversationId}", conversationId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<ChatDto>>> GetChatsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("GetChatsAsync called with empty userId");
                return Error.Validation("USERID_IS_EMPTY", "UserId cannot be empty.");
            }

            try
            {
                _logger.LogInformation("Getting chats for user id {UserId}", userId);
                var conversations = await _unitOfWork.Conversations.WhereAsync(
                    x => x.User1Id == userId || x.User2Id == userId,
                    x => x.User1,
                    x => x.User2,
                    x => x.LastMessage!
                );

                var chatDtos = conversations
                    .Where(x => {
                        var isUser1 = x.User1Id == userId;
                        var friend = isUser1 ? x.User2 : x.User1;
                        return friend?.UserName != null;
                    })
                    .Select(x => {
                        var isUser1 = x.User1Id == userId;
                        var friend = isUser1 ? x.User2 : x.User1;

                        return new ChatDto
                        {
                            FriendId = friend.Id,
                            FriendUserName = friend.UserName == null ? throw new Exception("Friend UserName is null.") : friend.UserName,
                            FriendAvatarUrl = friend.AvatarUrl,
                            ConversationId = x.Id,
                            LastMessageId = x.LastMessageId,
                            LastMessageContent = x.LastMessage?.Content,
                            IsFriendSenderMessage = x.LastMessage?.SenderId == friend.Id,
                            LastMessageTimestamp = x.LastMessage?.Timestamp
                        };
                    }).ToList();

                _logger.LogInformation("Returning {Count} chats for user id {UserId}", chatDtos.Count, userId);
                return chatDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in GetChatsAsync for userId: {UserId}", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<PagedMessagesDto>> GetPagedMessagesFromMessageIdAsync(Guid? conversationId, Guid? fromMessageId, Guid? userId)
        {
            if (conversationId == null || conversationId == Guid.Empty)
            {
                _logger.LogWarning("GetPagedMessagesFromMessageIdAsync called with empty conversationId");
                return Error.Validation("CONVERSATIONID_IS_EMPTY", "ConversationId cannot be empty.");
            }

            // TESTS
            if (userId == null || userId == Guid.Empty)
            {
                _logger.LogWarning("GetPagedMessagesFromMessageIdAsync called with empty userId");
                return Error.Validation("USERID_IS_EMPTY", "UserId cannot be empty.");
            }

            if (fromMessageId == null || fromMessageId == Guid.Empty)
            {
                _logger.LogInformation("GetPagedMessagesFromMessageIdAsync called with empty fromMessageId, returning empty list");
                return new PagedMessagesDto
                {
                    Messages = Enumerable.Empty<MessageDto>(),
                    LastMessageReadId = null
                };
            }

            var convId = conversationId.Value;
            var msgId = fromMessageId.Value;
            var userIdValue = userId.Value;

            try
            {
                _logger.LogInformation("Checking existence of conversation with id {ConversationId}", convId);
                var conversation = await GetConversationByIdAsync(convId);
                if (conversation == null)
                {
                    _logger.LogWarning("Conversation with id {ConversationId} not found", convId);
                    return Error.NotFound("CONVERSATION_ID_NOT_FOUND", "ConversationId was not found.");
                }

                _logger.LogInformation("Getting paged messages from messageId {MessageId} for conversationId {ConversationId}", msgId, convId);
                var messages = await _GetPagedMessagesFromMessageIdAsync(convId, msgId);

                var messageDtos = _mapper.Map<List<MessageDto>>(messages);

                //TESTS
                var lastReadMessageId = await GetLastReadMessageIdAsync(convId, userIdValue);

                if (lastReadMessageId.IsSuccess != true || lastReadMessageId.Value == null)
                {
                    _logger.LogError("Error retrieving last read message id for conversation {ConversationId}: {ErrorMessage}", convId, lastReadMessageId.Error?.Description);
                    return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
                }

                _logger.LogInformation("Returning {Count} messages", messageDtos.Count);
                return new PagedMessagesDto
                {
                    Messages = messageDtos,
                    LastMessageReadId = lastReadMessageId.Value.Value
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in GetPagedMessagesFromMessageIdAsync for conversationId: {ConversationId}, fromMessageId: {FromMessageId}", convId, msgId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<Result> SaveMessageAsync(Message message)
        {
            try
            {
                _logger.LogInformation("Saving message with id {MessageId} in conversation {ConversationId}", message.Id, message.ConversationId);
                await _SaveMessageAsync(message);
                await UpdateLastMessageInConversationAsync(message);
                _logger.LogInformation("Message saved and conversation last message updated");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in SaveMessageAsync for messageId: {MessageId}", message.Id);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        private async Task<List<Message>> _GetPagedMessagesFromMessageIdAsync(Guid conversationId, Guid fromMessageId)
        {
            var messages = await _unitOfWork.Messages.GetPagedMessagesFromMessageIdAsync(
                conversationId,
                fromMessageId,
                _messagesPageSize
                );

            return messages.ToList();
        }

        //TO DO: finish getlastmessageidasync and connect to getPagedMessagesFromMessageIdAsync

        private async Task<ResultT<Guid?>> GetLastReadMessageIdAsync(Guid conversationId, Guid recipientUserId)
        {
            var conversation = await _unitOfWork.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId);

            if (conversation != null)
            {
                return conversation.User1Id == recipientUserId
                    ? conversation.User2LastReadMessageId
                    : conversation.User1LastReadMessageId;
            }

            return Error.NotFound("CONVERSATION_NOT_FOUND", "Conversation was not found.");
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

        private async Task<Conversation?> GetConversationAsync(Guid user1Id, Guid user2Id)
        {
            return await _unitOfWork.Conversations.FirstOrDefaultAsync(x =>
                x.User1Id == user1Id && x.User2Id == user2Id ||
                x.User1Id == user2Id && x.User2Id == user1Id);
        }

        private async Task<Conversation?> GetConversationByIdAsync(Guid conversationId)
        {
            return await _unitOfWork.Conversations.FirstOrDefaultAsync(x => x.Id == conversationId);
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
