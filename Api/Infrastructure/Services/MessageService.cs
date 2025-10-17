using Application.DTOs;
using Application.Interfaces;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MessageService> _logger;

        public MessageService(UserManager<UserAccount> userManager, IUnitOfWork unitOfWork, IMapper mapper, ILogger<MessageService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
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
                    Conversation = conversation,
                    SenderId = userId,
                    Sender = _mapper.Map<User>(user),
                    Content = content,
                    Timestamp = DateTime.UtcNow
                };

                conversation.LastMessage = message;
                conversation.LastMessageTime = message.Timestamp;
                conversation.LastMessageId = message.Id;

                await _unitOfWork.Messages.AddAsync(message);
                _unitOfWork.Conversations.Update(conversation);
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
    }
}
