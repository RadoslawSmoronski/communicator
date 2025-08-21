using AutoMapper;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts.Dtos.Chat;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Models.Chat;
using ChatCommunicator.Infrastructure.UnitOfWork;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ChatCommunicator.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;
        private readonly IUsersConnectionService _usersConnectionService;
        private readonly ILogger<ChatService> _logger;

        private readonly int _messagesPageSize = 10;

        public ChatService(IUnitOfWork unitOfWork,
            UserManager<UserAccount> userManager,
            IMapper mapper,
            IUsersConnectionService usersConnectionService,
            ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _usersConnectionService = usersConnectionService;
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

                var onlineUsers = await _usersConnectionService.GetOnlineUsersIdAsync();

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
                            IsFriendOnline = onlineUsers.Any(x => x == friend.Id),
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

        public async Task<ResultT<ExtendedPagedMessagesDto>> GetPagedMessagesFromMessageIdAsync(Guid conversationId, Guid userId, Guid? fromMessageId)
        {
            if (conversationId == Guid.Empty)
            {
                _logger.LogWarning("GetPagedMessagesFromMessageIdAsync called with empty conversationId");
                return Error.Validation("CONVERSATIONID_IS_EMPTY", "ConversationId cannot be empty.");
            }

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("GetPagedMessagesFromMessageIdAsync called with empty userId");
                return Error.Validation("USERID_IS_EMPTY", "UserId cannot be empty.");
            }

            if (fromMessageId == null || fromMessageId == Guid.Empty)
            {
                _logger.LogInformation("GetPagedMessagesFromMessageIdAsync called with empty fromMessageId, returning empty list");
                
                var pagedMessagesDto = new PagedMessagesDto
                {
                    Messages = Enumerable.Empty<MessageDto>(),
                    LastFriendReadMessageId = null
                };

                return new ExtendedPagedMessagesDto
                {
                    PagedMessagesDto = pagedMessagesDto,
                    RecipientConnectionsId = null
                };
            }

            var convId = conversationId;
            var msgId = fromMessageId.Value;

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

                var lastFriendReadMessageId = _GetFriendLastReadMessage(userId, conversation);

                var setUserLastMessageResult = await SetAndGetUserLastReadMessageAsync(userId, conversationId);

                if (!setUserLastMessageResult.IsSuccess)
                {
                    var error = setUserLastMessageResult.Error;

                    if (error == null)
                    {
                        throw new Exception("An unknown error occurred while setting and retrieving the last friend read message.");
                    }

                    if(error.Code != "LAST_FRIEND_MESSAGE_NOT_FOUND")
                    {
                        return error;
                    }
                }

                _logger.LogInformation("Returning {Count} messages", messageDtos.Count);

                var pagedMessagesDto = new PagedMessagesDto
                {
                    Messages = messageDtos,
                    LastFriendReadMessageId = lastFriendReadMessageId
                };

                var recipientId = conversation.User1Id == userId ? conversation.User2Id : conversation.User1Id; 
                var recipientConnectionsId = _usersConnectionService.GetUserConnectionsId(recipientId);

                if (recipientConnectionsId != null && recipientConnectionsId.Count < 1) recipientConnectionsId = null;

                return new ExtendedPagedMessagesDto
                {
                    PagedMessagesDto = pagedMessagesDto,
                    RecipientConnectionsId = recipientConnectionsId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in GetPagedMessagesFromMessageIdAsync for conversationId: {ConversationId}, fromMessageId: {FromMessageId}", convId, msgId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<Guid>> SetAndGetUserLastReadMessageAsync(Guid userId, Guid conversationId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("SetAndGetUserLastReadMessageAsync called with empty userId");
                return Error.Validation("USERID_IS_EMPTY", "UserId cannot be empty.");
            }

            if (conversationId == Guid.Empty)
            {
                _logger.LogWarning("SetAndGetUserLastReadMessageAsync called with empty conversationId");
                return Error.Validation("CONVERSATIONID_IS_EMPTY", "ConversationId cannot be empty.");
            }

            try
            {
                _logger.LogInformation("Setting and retrieving last read message for userId {UserId} in conversationId {ConversationId}", userId, conversationId);
                var result = await _SetAndGetUserLastReadMessageAsync(userId, conversationId);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully set and retrieved last read message for userId {UserId} in conversationId {ConversationId}", userId, conversationId);
                    return result;
                }

                _logger.LogWarning("Failed to set and retrieve last read message for userId {UserId} in conversationId {ConversationId}", userId, conversationId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in SetAndGetUserLastReadMessageAsync for userId: {UserId}, conversationId: {ConversationId}", userId, conversationId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<MessageDto>> SendMessageAsync(Guid userId, Guid conversationId, string content)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("SendMessageAsync called with empty userId");
                return Error.Validation("USERID_IS_EMPTY", "UserId cannot be empty.");
            }

            if (conversationId == Guid.Empty)
            {
                _logger.LogWarning("SendMessageAsync called with empty conversationId");
                return Error.Validation("CONVERSATIONID_IS_EMPTY", "ConversationId cannot be empty.");
            }

            try
            {
                _logger.LogInformation("Retrieving user with id {UserId}", userId);
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("User with id {UserId} not found", userId);
                    return Error.NotFound("USER_NOT_FOUND", "User was not found.");
                }

                _logger.LogInformation("Retrieving conversation with id {ConversationId}", conversationId);
                var conversation = await GetConversationByIdAsync(conversationId);

                if (conversation == null)
                {
                    _logger.LogWarning("Conversation with id {ConversationId} not found", conversationId);
                    return Error.NotFound("CONVERSATION_NOT_FOUND", "Conversation was not found.");
                }

                var message = new Message()
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    Conversation = conversation,
                    SenderId = userId,
                    Sender = user,
                    Content = content,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Updating conversation {ConversationId} with new last message {MessageId}", conversationId, message.Id);
                conversation.LastMessage = message;
                conversation.LastMessageTime = message.Timestamp;
                conversation.LastMessageId = message.Id;

                _logger.LogInformation("Saving new message with id {MessageId} for conversation {ConversationId}", message.Id, conversationId);
                await _unitOfWork.Messages.AddAsync(message);
                _unitOfWork.Conversations.Update(conversation);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("Message {MessageId} saved and conversation {ConversationId} updated successfully", message.Id, conversationId);

                return _mapper.Map<MessageDto>(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in SendMessageAsync for userId: {UserId}, conversationId: {ConversationId}", userId, conversationId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        private async Task<ResultT<Guid>> _SetAndGetUserLastReadMessageAsync(Guid userId, Guid conversationId)
        {
            _logger.LogInformation("Retrieving conversation with id {ConversationId} for userId {UserId}", conversationId, userId);
            var conversation = await GetConversationByIdAsync(conversationId);

            if (conversation == null)
            {
                _logger.LogWarning("Conversation with id {ConversationId} not found for userId {UserId}", conversationId, userId);
                return Error.NotFound("CONVERSATION_NOT_FOUND", "Conversation was not found.");
            }

            _logger.LogInformation("Retrieving last friend message for userId {UserId} in conversationId {ConversationId}", userId, conversationId);
            var message = await _unitOfWork.Messages.GetUserLastFriendMessageAsync(conversationId, userId);

            if (message == null)
            {
                _logger.LogWarning("No last friend message found for userId {UserId} in conversationId {ConversationId}", userId, conversationId);
                return Error.NotFound("LAST_FRIEND_MESSAGE_NOT_FOUND", "Last friend message was not found.");
            }

            if (conversation.User1Id == userId)
            {
                conversation.User1LastReadMessageId = message.Id;
            }
            else
            {
                conversation.User2LastReadMessageId = message.Id;
            }

            _logger.LogInformation("Updating conversation with id {ConversationId} to set last read message for userId {UserId}", conversationId, userId);
            _unitOfWork.Conversations.Update(conversation);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("Successfully updated last read message for userId {UserId} in conversationId {ConversationId}", userId, conversationId);
            return message.Id;
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

        private Guid? _GetFriendLastReadMessage(Guid userId, Conversation conversation)
        {
            return conversation.User1Id == userId ? conversation.User2LastReadMessageId : conversation.User1LastReadMessageId;
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
