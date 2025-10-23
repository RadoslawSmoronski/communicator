using Application.DTOs;
using Application.Interfaces;
using Application.Repositories;
using Application.Settings;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Result;

namespace Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MessageService> _logger;
        private readonly IUsersConnectionService _usersConnectionService;

        private readonly MessagesSettings _messagesSettings;

        public MessageService(UserManager<UserAccount> userManager, IUnitOfWork unitOfWork, IMapper mapper, ILogger<MessageService> logger, IOptions<MessagesSettings> options, IUsersConnectionService usersConnectionService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _messagesSettings = options.Value;
            _usersConnectionService = usersConnectionService;
        }

        public async Task<Result<MessageDto>> SendMessageAsync(Guid userId, Guid conversationId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("SendMessageAsync validation failed: empty content. userId={UserId}, conversationId={ConversationId}", userId, conversationId);
                    return Error.Validation("Message.Content.Required", "Message content must be provided.");
                }

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                {
                    _logger.LogWarning("SendMessageAsync user not found. userId={UserId}", userId);
                    return Error.NotFound("User.NotFound", "The specified user was not found.");
                }

                var conversation = await _unitOfWork.Conversations.GetConversationByIdAsync(conversationId);
                if (conversation is null)
                {
                    _logger.LogWarning("SendMessageAsync conversation not found. conversationId={ConversationId}", conversationId);
                    return Error.NotFound("Conversation.NotFound", "The specified conversation was not found.");
                }

                if (conversation.User1Id != userId && conversation.User2Id != userId)
                {
                    _logger.LogWarning("SendMessageAsync forbidden: user is not a participant. userId={UserId}, conversationId={ConversationId}", userId, conversationId);
                    return Error.Forbidden("Conversation.AccessDenied", "You are not a participant of this conversation.");
                }

                var message = new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversationId,
                    SenderId = userId,
                    Content = content,
                    Timestamp = DateTime.UtcNow
                };

                conversation.LastMessageId = message.Id;
                conversation.LastMessageTime = message.Timestamp;

                await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.Conversations.UpdateAsync(conversation);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Message sent successfully. messageId={MessageId}, conversationId={ConversationId}, userId={UserId}", message.Id, conversationId, userId);

                return _mapper.Map<MessageDto>(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendMessageAsync failed. userId={UserId}, conversationId={ConversationId}", userId, conversationId);
                return Error.Failure("Message.Send.Failed", "An unexpected error occurred while sending the message.");
            }
        }
        public async Task<Result<Guid>> SetAndGetUserLastReadMessageAsync(Guid userId, Guid conversationId)
        {
            try
            {
                _logger.LogDebug("SetAndGetUserLastReadMessageAsync started. userId={UserId}, conversationId={ConversationId}", userId, conversationId);

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                {
                    _logger.LogWarning("SetAndGetUserLastReadMessageAsync user not found. userId={UserId}", userId);
                    return Error.NotFound("User.NotFound", "The specified user was not found.");
                }

                var conversation = await _unitOfWork.Conversations.GetConversationByIdAsync(conversationId);
                if (conversation is null)
                {
                    _logger.LogWarning("SetAndGetUserLastReadMessageAsync conversation not found. conversationId={ConversationId}", conversationId);
                    return Error.NotFound("Conversation.NotFound", "The specified conversation was not found.");
                }

                if (conversation.User1Id != userId && conversation.User2Id != userId)
                {
                    _logger.LogWarning("SetAndGetUserLastReadMessageAsync forbidden: user is not a participant. userId={UserId}, conversationId={ConversationId}", userId, conversationId);
                    return Error.Forbidden("Conversation.AccessDenied", "You are not a participant of this conversation.");
                }

                var message = await _unitOfWork.Messages.GetUserLastFriendMessageAsync(conversationId, userId);
                if (message is null)
                {
                    _logger.LogWarning("SetAndGetUserLastReadMessageAsync last friend message not found. userId={UserId}, conversationId={ConversationId}", userId, conversationId);
                    return Error.NotFound("Message.NotFound", "No messages from the other participant were found.");
                }

                if (conversation.User1Id == userId)
                {
                    conversation.User1LastReadMessageId = message.Id;
                }
                else
                {
                    conversation.User2LastReadMessageId = message.Id;
                }

                await _unitOfWork.Conversations.UpdateAsync(conversation);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("SetAndGetUserLastReadMessageAsync succeeded. conversationId={ConversationId}, userId={UserId}, lastReadMessageId={MessageId}", conversationId, userId, message.Id);

                return message.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetAndGetUserLastReadMessageAsync failed. userId={UserId}, conversationId={ConversationId}", userId, conversationId);
                return Error.Failure("Message.LastRead.Set.Failed", "An unexpected error occurred while setting the last read message.");
            }
        }

        public async Task<Result<ExtendedPagedMessagesDto>> GetPagedMessagesFromMessageIdAsync(Guid conversationId, Guid userId, Guid? fromMessageId)
        {

            if (fromMessageId is null || fromMessageId == Guid.Empty)
            { 
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
                var conversation = await _unitOfWork.Conversations.GetConversationByIdAsync(convId);
                if (conversation == null)
                {
                    return Error.NotFound("", "");
                }

               
                var messages = await _GetPagedMessagesFromMessageIdAsync(convId, msgId);

                var messageDtos = _mapper.Map<List<MessageDto>>(messages);

                var lastFriendReadMessageId = _GetFriendLastReadMessage(userId, conversation);

                var setUserLastMessageResult = await SetAndGetUserLastReadMessageAsync(userId, conversationId);
                Guid? userReadMessageId = null;

                if (setUserLastMessageResult.IsSuccess)
                {
                    userReadMessageId = setUserLastMessageResult.Value;
                }
                else
                {
                    var error = setUserLastMessageResult.Error;

                    if (error == null)
                    {
                        throw new Exception("An unknown error occurred while setting and retrieving the last friend read message.");
                    }

                    if (error.Code != "LAST_FRIEND_MESSAGE_NOT_FOUND")
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

                if (recipientConnectionsId != null && recipientConnectionsId.Count < 1)
                {
                    recipientConnectionsId = null;
                    userReadMessageId = null;
                }

                return new ExtendedPagedMessagesDto
                {
                    PagedMessagesDto = pagedMessagesDto,
                    RecipientConnectionsId = recipientConnectionsId,
                    UserReadMessageId = userReadMessageId
                };

                throw new Exception();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred in GetPagedMessagesFromMessageIdAsync for conversationId: {ConversationId}, fromMessageId: {FromMessageId}", convId, msgId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        private async Task<List<Message>> _GetPagedMessagesFromMessageIdAsync(Guid conversationId, Guid fromMessageId)
        {
            var messages = await _unitOfWork.Messages.GetPagedMessagesFromMessageIdAsync(
                conversationId,
                fromMessageId,
                _messagesSettings.PageSize
                );

            return messages.ToList();
        }

        private Guid? _GetFriendLastReadMessage(Guid userId, Conversation conversation)
        {
            return conversation.User1Id == userId ? conversation.User2LastReadMessageId : conversation.User1LastReadMessageId;
        }
    }
}
