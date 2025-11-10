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

                if (await _unitOfWork.Friendships.IsExistAsync(user1Id, user2Id))
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

        public async Task<Result<List<Friend>>> GetUserFriendAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.UserName == null)
                {
                    _logger.LogWarning("User not found or has no username. UserId: {UserId}", userId);
                    return Error.NotFound("Friendship.UserNotFound", "User not found or has no username.");
                }

                var friends = await _unitOfWork.Friendships.GetAllAsync(user.Id);

                _logger.LogInformation("Fetched {Count} friendships for UserId: {UserId}", friends.Count, userId);

                return friends.Select(x => {
                    var isUser1 = x.User1Id == userId;
                    var friend = isUser1 ? x.User2 : x.User1;

                    return new Friend()
                    {
                        Id = friend!.Id,
                        UserName = friend.UserName,
                        Email = friend.Email,
                        AvatarUrl = friend.AvatarUrl,
                        FriendshipId = x.Id,
                        FriendshipCreatedAt = x.CreatedAt,
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get friendships for UserId: {UserId}", userId);
                return Error.Failure("Friendship.GetFailed", "Failed to get friendships due to an unexpected error.");
            }
        }

        public async Task<Result> DeleteAsync(Guid friendshipId)
        {
            try
            {
                var friendship = await _unitOfWork.Friendships.GetAsync(friendshipId);

                if (friendship == null)
                {
                    _logger.LogWarning("Friendship not found. FriendshipId: {FriendshipId}", friendshipId);
                    return Error.NotFound("Friendship.NotFound", "Friendship with the specified ID does not exist.");
                }

                await _unitOfWork.Friendships.DeleteAsync(friendship.Id);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Friendship deleted. FriendshipId: {FriendshipId}", friendshipId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete friendship. FriendshipId: {FriendshipId}", friendshipId);
                return Error.Failure("Friendship.DeleteFailed", "Failed to delete friendship due to an unexpected error.");
            }
        }

        public async Task<Result> IsExistAsync(Guid user1Id, Guid user2Id)
        {
            try
            {
                if (await _unitOfWork.Friendships.IsExistAsync(user1Id, user2Id))
                {
                    return Result.Success();
                }
                else
                {
                    return Error.NotFound("Friendship.NotExist", "Friendship does not exist between the specified users.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking existence of friendship between User1Id: {User1Id} and User2Id: {User2Id}", user1Id, user2Id);
                return Error.Failure("Friendship.IsExistFailed", "Failed to check friendship existence due to an unexpected error.");
            }
        }
    }
}
