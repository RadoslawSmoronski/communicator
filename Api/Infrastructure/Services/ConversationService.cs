using Application.DTOs;
using Application.Interfaces;
using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Infrastructure.Services
{
    public class ConversationService : IConversationService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly ILogger<ConversationService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ConversationService(UserManager<UserAccount> userManager, ILogger<ConversationService> logger, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _logger = logger;
            _unitOfWork = unitOfWork;
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

                var conversation = await GetConversationAsync(userId, friendId);

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

                return Result<Conversation>.Success(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get or create conversation between {UserId} and {FriendId}.", userId, friendId);
                return Error.Failure("Conversation.Failure", "An unexpected error occurred while getting or creating the conversation.");
            }
        }

        private async Task<Conversation?> GetConversationAsync(Guid user1Id, Guid user2Id)
            => await _unitOfWork.Conversations.FirstOrDefaultAsync(x =>
                (x.User1Id == user1Id && x.User2Id == user2Id) ||
                (x.User1Id == user2Id && x.User2Id == user1Id));

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

        //Task<Result<List<ConversationDto>>> GetAsync(Guid userId)
        //{

        //}

    }
}
