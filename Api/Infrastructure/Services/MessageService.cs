using Application.Common.Interfaces;
using Application.Contracts.Chat;
using Application.Repositories;
using Application.Common.Settings;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Result;
using Infrastructure.Entities;

namespace Infrastructure.Services
{
    public class MessageService(
        UserManager<UserAccount> userManager,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MessageService> logger,
        IOptions<MessagesSettings> options)
        : IMessageService
    {
        private readonly UserManager<UserAccount> _userManager = userManager;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<MessageService> _logger = logger;
        private readonly MessagesSettings _messagesSettings = options.Value;

        public async Task<Result<MessageReceivedEvent>> SendMessageAsync(Guid userId, Guid conversationId, string content)
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
                    CreatedAt = DateTime.UtcNow
                };

                conversation.LastMessageId = message.Id;
                conversation.LastMessageTime = message.CreatedAt;

                await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.Conversations.UpdateAsync(conversation);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Message sent successfully. messageId={MessageId}, conversationId={ConversationId}, userId={UserId}", message.Id, conversationId, userId);

                return _mapper.Map<MessageReceivedEvent>(message);
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

        public async Task<Result<List<Message>>> GetPagedMessagesFromMessageIdAsync(Guid conversationId, Guid userId, Guid? fromMessageId)
        {
            _logger.LogDebug("GetPagedMessagesFromMessageIdAsync started. conversationId={ConversationId}, userId={UserId}, fromMessageId={FromMessageId}",
                conversationId, userId, fromMessageId);

            if (fromMessageId is null || fromMessageId == Guid.Empty)
            {
                _logger.LogWarning("GetPagedMessagesFromMessageIdAsync validation failed: fromMessageId missing. conversationId={ConversationId}, userId={UserId}",
                    conversationId, userId);
                return Error.Validation("Message.Paging.StartId.Required", "Start message id must be provided.");
            }

            var startMessageId = fromMessageId.Value;

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user is null)
                {
                    _logger.LogWarning("GetPagedMessagesFromMessageIdAsync user not found. userId={UserId}", userId);
                    return Error.NotFound("User.NotFound", "The specified user was not found.");
                }

                var conversation = await _unitOfWork.Conversations.GetConversationByIdAsync(conversationId);
                if (conversation is null)
                {
                    _logger.LogWarning("GetPagedMessagesFromMessageIdAsync conversation not found. conversationId={ConversationId}", conversationId);
                    return Error.NotFound("Conversation.NotFound", "The specified conversation was not found.");
                }

                if (conversation.User1Id != userId && conversation.User2Id != userId)
                {
                    _logger.LogWarning("GetPagedMessagesFromMessageIdAsync forbidden: user not participant. userId={UserId}, conversationId={ConversationId}",
                        userId, conversationId);
                    return Error.Forbidden("Conversation.AccessDenied", "You are not a participant of this conversation.");
                }

                var messages = await GetPagedMessagesFromMessageIdAsync(conversationId, startMessageId);

                _logger.LogInformation("GetPagedMessagesFromMessageIdAsync succeeded. conversationId={ConversationId}, userId={UserId}, returnedMessages={Count}",
                    conversationId, userId, messages.Count);

                return messages;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "GetPagedMessagesFromMessageIdAsync message start id not found. conversationId={ConversationId}, fromMessageId={FromMessageId}",
                    conversationId, startMessageId);
                return Error.NotFound("Message.NotFound", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPagedMessagesFromMessageIdAsync failed. conversationId={ConversationId}, fromMessageId={FromMessageId}, userId={UserId}",
                    conversationId, startMessageId, userId);
                return Error.Unknown("Message.Paging.Failed", "An internal server error occurred.");
            }
        }

        private async Task<List<Message>> GetPagedMessagesFromMessageIdAsync(Guid conversationId, Guid fromMessageId)
        {
            var messages = await _unitOfWork.Messages.GetPagedMessagesFromMessageIdAsync(
                conversationId,
                fromMessageId,
                _messagesSettings.PageSize
                );

            return messages.ToList();
        }
    }
}
