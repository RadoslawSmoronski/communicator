using Application.Interfaces;
using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Infrastructure.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FriendshipService> _logger;

        public FriendshipService(UserManager<UserAccount> userManager, IUnitOfWork unitOfWork, ILogger<FriendshipService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Guid>> AddAsync(Guid user1Id, Guid user2Id)
        {
            if (user1Id == user2Id)
            {
                _logger.LogWarning("User attempted to add themselves as a friend. UserId: {UserId}", user1Id);
                return Error.Validation("Friendship.SameUser", "Cannot add yourself as a friend.");
            }

            try
            {
                var user1 = await _userManager.FindByIdAsync(user1Id.ToString());
                if (user1 == null || user1.UserName == null)
                {
                    _logger.LogWarning("User1 not found or has no username. User1Id: {User1Id}", user1Id);
                    return Error.NotFound("Friendship.User1NotFound", "User1 not found.");
                }

                var user2 = await _userManager.FindByIdAsync(user2Id.ToString());
                if (user2 == null || user2.UserName == null)
                {
                    _logger.LogWarning("User2 not found or has no username. User2Id: {User2Id}", user2Id);
                    return Error.NotFound("Friendship.User2NotFound", "User2 not found.");
                }

                if (await IsFriendshipExistAsync(user1Id, user2Id))
                {
                    _logger.LogWarning("Friendship already exists between User1Id: {User1Id} and User2Id: {User2Id}", user1Id, user2Id);
                    return Error.Conflict("Friendship.AlreadyExists", "Friendship already exists.");
                }

                var friendship = new Friendship
                {
                    User1Id = user1.Id,
                    User2Id = user2.Id
                };

                await _unitOfWork.Friendships.AddAsync(friendship);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Friendship created between User1Id: {User1Id} and User2Id: {User2Id}", user1Id, user2Id);
                return friendship.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add friendship between User1Id: {User1Id} and User2Id: {User2Id}", user1Id, user2Id);
                return Error.Failure("Friendship.AddFailed", "Failed to add friendship due to an unexpected error.");
            }
        }

        private async Task<bool> IsFriendshipExistAsync(Guid userId1, Guid userId2) => 
             await _unitOfWork.Friendships
                .AnyAsync(x => x.User1Id == userId1 && x.User2Id == userId2
                || x.User1Id == userId2 && x.User2Id == userId1);
    }
}
