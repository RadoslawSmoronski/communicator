using Application.Common.Interfaces;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Infrastructure.Services
{
    public class ConversationService : IConversationService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly ILogger<ConversationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ConversationService(UserManager<UserAccount> userManager, ILogger<ConversationService> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<Conversation>> GetOrCreateAsync(Guid userId, Guid friendId)
        {
            if (userId == friendId)
            {
                _logger.LogWarning("User {UserId} tried to start a conversation with themselves.", userId);
                return Error.Validation("Conversation.Self", "Cannot start a conversation with yourself.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user is null || user.UserName is null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return Error.NotFound("User.NotFound", $"User with ID {userId} not found.");
                }

                var friendUser = await _userManager.FindByIdAsync(friendId.ToString());

                if (friendUser is null || friendUser.UserName is null)
                {
                    _logger.LogWarning("Friend user not found: {FriendId}", friendId);
                    return Error.NotFound("Friend.NotFound", $"Friend with ID {friendId} not found.");
                }

                var conversation = await _unitOfWork.Conversations.GetConversationByUsersIdAsync(userId, friendId);

                if (conversation == null)
                {
                    _logger.LogInformation("No existing conversation between {UserId} and {FriendId}. Creating new.", userId, friendId);
                    conversation = await CreateConversationAsync(user, friendUser);
                    _logger.LogInformation("Created new conversation {ConversationId} between {UserId} and {FriendId}.", conversation.Id, userId, friendId);
                }
                else
                {
                    _logger.LogInformation("Found existing conversation {ConversationId} between {UserId} and {FriendId}.", conversation.Id, userId, friendId);
                }

                return conversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get or create conversation between {UserId} and {FriendId}.", userId, friendId);
                return Error.Failure("Conversation.Failure", "An unexpected error occurred while getting or creating the conversation.");
            }
        }

        private async Task<Conversation> CreateConversationAsync(UserAccount user1, UserAccount user2)
        {
            var conversation = new Conversation()
            {
                User1Id = user1.Id,
                User2Id = user2.Id
            };

            await _unitOfWork.Conversations.AddAsync(conversation);
            await _unitOfWork.SaveAsync();

            return conversation;
        }

        public Result<List<Conversation>> GetAll(Guid userId)
        {
            try
            {
                var result = _unitOfWork.Conversations.GetUserAll(userId);
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all conversations for user {UserId}.", userId);
                return Error.Failure("Conversation.GetAll.Failure", "An unexpected error occurred while retrieving conversations.");
            }
        }

        public async Task<Result<Conversation>> GetByIdAsync(Guid conversationId)
        {
            try
            {
                _logger.LogInformation("Fetching conversation by id {ConversationId}.", conversationId);

                var result = await _unitOfWork.Conversations.GetConversationByIdAsync(conversationId);

                if (result is null)
                {
                    _logger.LogWarning("Conversation not found: {ConversationId}", conversationId);
                    return Error.NotFound("Conversation.NotFound", $"Conversation with ID {conversationId} not found.");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get conversation by id {ConversationId}.", conversationId);
                return Error.Failure("Conversation.GetById.Failure", "An unexpected error occurred while retrieving the conversation.");
            }
        }
    }
}
