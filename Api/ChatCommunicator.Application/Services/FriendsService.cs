using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.FriendsController;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Models.Friendship;
using ChatCommunicator.Infrastructure.UnitOfWork;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace ChatCommunicator.Application.Managers
{
    public class FriendsService : IFriendsService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FriendsService> _logger;
        private readonly IUsersConnectionService _usersConnectionService;
        private readonly IUserAvatarService _userAvatarService;

        public FriendsService(UserManager<UserAccount> userManager,
            IUnitOfWork unitOfWork,
            ILogger<FriendsService> logger,
            IUserAvatarService userAvatarService,
            IUsersConnectionService usersConnectionService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userAvatarService = userAvatarService;
            _usersConnectionService = usersConnectionService;
        }

        public async Task<ResultT<Guid>> SendInviteAsync(Guid senderId, Guid recipientId)
        {
            _logger.LogInformation("SendInviteAsync called with senderId: {SenderId}, recipientId: {RecipientId}", senderId, recipientId);

            if (senderId == Guid.Empty || recipientId == Guid.Empty)
            {
                _logger.LogWarning("SendInviteAsync validation failed: senderId or recipientId is empty");
                return Error.Validation("SENDERID_IS_EMPTY", "SenderId or RecipientId cannot be empty.");
            }

            if (senderId == recipientId)
            {
                _logger.LogWarning("SendInviteAsync validation failed: senderId and recipientId are the same");
                return Error.Validation("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "Sender ID and Recipient ID must be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId.ToString());

                if (senderUser == null || senderUser.UserName == null)
                {
                    _logger.LogWarning("SendInviteAsync: Sender user not found for id {SenderId}", senderId);
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser was not found.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId.ToString());

                if (recipientUser == null || recipientUser.UserName == null)
                {
                    _logger.LogWarning("SendInviteAsync: Recipient user not found for id {RecipientId}", recipientId);
                    return Error.NotFound("RECIPIENTUSER_NOT_FOUND", "RecipientUser was not found.");
                }

                if (await IsFriendsInvitationExists(senderId, recipientId))
                {
                    _logger.LogWarning("SendInviteAsync: Invitation already exists between {SenderId} and {RecipientId}", senderId, recipientId);
                    return Error.Conflict("FRIENDS_INVITATION_EXISTS", "An invitation has already exist.");
                }

                var friendshipInvitationId = await SendFriendshipInviteAsync(senderUser, recipientUser);
                if (friendshipInvitationId.IsSuccess)
                {
                    _logger.LogInformation("SendInviteAsync: Invitation sent from {SenderId} to {RecipientId}", senderId, recipientId);
                    return friendshipInvitationId;
                }
                else
                {
                    _logger.LogError("SendInviteAsync: Failed to send invitation from {SenderId} to {RecipientId}. Error: {Error}", senderId, recipientId, friendshipInvitationId.Error?.Description);
                    return friendshipInvitationId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendInviteAsync: Internal server error for senderId {SenderId} and recipientId {RecipientId}", senderId, recipientId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<FriendshipInvitationDto>>> GetInvitationsAsync(Guid userId)
        {
            _logger.LogInformation("GetInvitationsAsync called with userId: {UserId}", userId);

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("GetInvitationsAsync validation failed: userId is empty");
                return Error.Validation("USERID_IS_EMPTY", "UserId cannot be null or empty.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.UserName == null)
                {
                    _logger.LogWarning("GetInvitationsAsync: User not found for id {UserId}", userId);
                    return Error.NotFound("USERID_NOT_FOUND", "User was not found.");
                }

                var list = await GetUserInvitationsAsync(user);
                _logger.LogInformation("GetInvitationsAsync: Found {Count} invitations for userId {UserId}", list.Count, userId);

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetInvitationsAsync: Internal server error for userId {UserId}", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<UserToInviteDto>>> GetUsersToInviteByTextAsync(Guid userId, string text)
        {
            _logger.LogInformation("GetUsersToInviteByTextAsync called with userId: {UserId}, text: {Text}", userId, text);

            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("GetUsersToInviteByTextAsync validation failed: userId or text is empty");
                return Error.Validation("USERID_OR_TEXT_ARE_EMPTY", "UserId or text cannot be null or empty.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.UserName == null)
                {
                    _logger.LogWarning("GetUsersToInviteByTextAsync: User not found for id {UserId}", userId);
                    return Error.NotFound("USERID_NOT_FOUND", "User was not found.");
                }

                var list = await GetUsersToInviteByTextAsync(user, text);
                _logger.LogInformation("GetUsersToInviteByTextAsync: Found {Count} users to invite for userId {UserId}", list.Count, userId);

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUsersToInviteByTextAsync: Internal server error for userId {UserId}", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<Result> DecelineInviteAsync(Guid senderId, Guid recipientId)
        {
            _logger.LogInformation("DecelineInviteAsync called with senderId: {SenderId}, recipientId: {RecipientId}", senderId, recipientId);

            if (senderId == Guid.Empty || recipientId == Guid.Empty)
            {
                _logger.LogWarning("DecelineInviteAsync validation failed: senderId or recipientId is empty");
                return Error.Validation("SENDERID_IS_EMPTY", "SenderId and RecipientId cannot be empty.");
            }

            if (senderId == recipientId)
            {
                _logger.LogWarning("DecelineInviteAsync validation failed: senderId and recipientId are the same");
                return Error.Validation("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "SenderId and RecipientId must be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId.ToString());

                if (senderUser == null || senderUser.UserName == null)
                {
                    _logger.LogWarning("DecelineInviteAsync: Sender user not found for id {SenderId}", senderId);
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser was not found.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId.ToString());

                if (recipientUser == null || recipientUser.UserName == null)
                {
                    _logger.LogWarning("DecelineInviteAsync: Recipient user not found for id {RecipientId}", recipientId);
                    return Error.NotFound("RECIPIENTUSER_NOT_FOUND", "RecipientUser was not found.");
                }

                var result = await DeleteInviteAsync(senderUser, recipientUser);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("DecelineInviteAsync: Invitation declined between {SenderId} and {RecipientId}", senderId, recipientId);
                    return Result.Success();
                }

                if (result.Error != null)
                {
                    _logger.LogWarning("DecelineInviteAsync: Invitation not found between {SenderId} and {RecipientId}", senderId, recipientId);
                    return Error.NotFound("INVITATION_NOT_FOUND", "Invitation was not found.");
                }

                _logger.LogError("DecelineInviteAsync: Unknown error declining invitation between {SenderId} and {RecipientId}", senderId, recipientId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DecelineInviteAsync: Internal server error for senderId {SenderId} and recipientId {RecipientId}", senderId, recipientId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<Guid>> AddFriendsAsync(Guid senderId, Guid recipientId)
        {
            _logger.LogInformation("AddFriendsAsync called with senderId: {SenderId}, recipientId: {RecipientId}", senderId, recipientId);

            if (senderId == Guid.Empty || recipientId == Guid.Empty)
            {
                _logger.LogWarning("AddFriendsAsync validation failed: senderId or recipientId is empty");
                return Error.Validation("SENDERID_IS_EMPTY", "SenderId and RecipientId cannot be null or empty.");
            }

            if (senderId == recipientId)
            {
                _logger.LogWarning("AddFriendsAsync validation failed: senderId and recipientId are the same");
                return Error.Validation("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "Sender ID and Recipient ID must be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId.ToString());

                if (senderUser == null || senderUser.UserName == null)
                {
                    _logger.LogWarning("AddFriendsAsync: Sender user not found for id {SenderId}", senderId);
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser was not found.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId.ToString());

                if (recipientUser == null || recipientUser.UserName == null)
                {
                    _logger.LogWarning("AddFriendsAsync: Recipient user not found for id {RecipientId}", recipientId);
                    return Error.NotFound("RECIPIENTUSER_NOT_FOUND", "RecipientUser was not found.");
                }

                if (await IsFriendsInvitationExists(senderId, recipientId) == false)
                {
                    _logger.LogWarning("AddFriendsAsync: Invitation not found between {SenderId} and {RecipientId}", senderId, recipientId);
                    return Error.NotFound("INVITATION_NOT_FOUND", "The invitation was not found.");
                }

                if (await IsFriendsExistAsync(senderId, recipientId))
                {
                    _logger.LogWarning("AddFriendsAsync: Friends already exist between {SenderId} and {RecipientId}", senderId, recipientId);
                    return Error.Conflict("FRIENDS_ALREADY_EXISTS", "This relationship has already exist.");
                }

                await DeleteInviteAsync(senderUser, recipientUser);
                var friendshipId = await AddFriendsAsync(senderUser, recipientUser);

                _logger.LogInformation("AddFriendsAsync: Friends added between {SenderId} and {RecipientId}", senderId, recipientId);
                return friendshipId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddFriendsAsync: Internal server error for senderId {SenderId} and recipientId {RecipientId}", senderId, recipientId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<SimpleUserWithAvatarDto>>> GetFriendsAsync(Guid userId)
        {
            _logger.LogInformation("GetFriendsAsync called with userId: {UserId}", userId);

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("GetFriendsAsync validation failed: userId is empty");
                return Error.Validation("USERID_IS_EMPTY", "UserId cannot be null or empty.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.UserName == null)
                {
                    _logger.LogWarning("GetFriendsAsync: User not found for id {UserId}", userId);
                    return Error.NotFound("USERID_NOT_FOUND", "User was not found.");
                }

                var list = await GetFriendsFromDbAsync(userId);
                _logger.LogInformation("GetFriendsAsync: Found {Count} friends for userId {UserId}", list.Count, userId);

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetFriendsAsync: Internal server error for userId {UserId}", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<bool> IsFriendsExistAsync(Guid userId1, Guid userId2)
        {
            _logger.LogInformation("IsFriendsExistAsync called with userId1: {UserId1}, userId2: {UserId2}", userId1, userId2);

            return await _unitOfWork.Friendships
                .AnyAsync(x => x.User1Id == userId1 && x.User2Id == userId2
                || x.User1Id == userId2 && x.User2Id == userId1);
        }

        public async Task<ResultT<List<string>>> GetUserOnlineFriendsConnectionsIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("GetUserOnlineFriendsConnectionsIdAsync validation failed: userId is empty");
                return Error.Validation("USERID_IS_EMPTY", "UserId cannot be null or empty.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.UserName == null)
                {
                    _logger.LogWarning("GetUserOnlineFriendsConnectionsIdAsync: User not found for id {UserId}", userId);
                    return Error.NotFound("USER_NOT_FOUND", "User was not found.");
                }

                var friends = await GetFriendsFromDbAsync(userId);
                var onlineFriendsConnections = new List<string>();

                foreach (var friend in friends)
                {
                    if (await _usersConnectionService.IsUserOnlineAsync(friend.Id))
                    {
                        var connections = _usersConnectionService.GetUserConnectionsId(friend.Id);
                        if (connections != null)
                        {
                            onlineFriendsConnections.AddRange(connections);
                        }
                    }
                }

                _logger.LogInformation("GetUserOnlineFriendsConnectionsIdAsync: Found {Count} online friends connections for userId {UserId}", onlineFriendsConnections.Count, userId);
                return onlineFriendsConnections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserOnlineFriendsConnectionsIdAsync: Internal server error for userId {UserId}", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }
        public async Task<Result> DeleteAsync(Guid friendshipId)
        {
            if (friendshipId == Guid.Empty)
            {
                _logger.LogWarning("DeleteAsync validation failed: friendshipId is empty");
                return Error.Validation("FRIENDSHIPID_IS_EMPTY", "FriendshipId cannot be null or empty.");
            }

            try
            {
                var friendship = await _GetFriendship(friendshipId);

                if (friendship == null)
                {
                    _logger.LogWarning("DeleteAsync: Friendship not found for id {FriendshipId}", friendshipId);
                    return Error.NotFound("FRIENDSHIP_NOT_FOUND", "Friendship was not found.");
                }

                _unitOfWork.Friendships.Delete(friendship);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("DeleteAsync: Friendship deleted successfully for id {FriendshipId}", friendshipId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync: Internal server error for friendshipId {FriendshipId}", friendshipId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        private async Task<Friendship?> _GetFriendship(Guid friendshipId)
        {
            _logger.LogInformation("GetFriendship called with friendshipId: {FriendshipId}", friendshipId);
            return await _unitOfWork.Friendships.FirstOrDefaultAsync(x => x.Id == friendshipId);
        }

        private async Task<bool> IsFriendsInvitationExists(Guid user1Id, Guid user2Id)
        {
            _logger.LogInformation("IsFriendsInvitationExists called with user1Id: {User1Id}, user2Id: {User2Id}", user1Id, user2Id);

            return await _unitOfWork.FriendshipInvitations.AnyAsync(x =>
            x.SenderId == user1Id && x.RecipientId == user2Id ||
            x.SenderId == user2Id && x.RecipientId == user1Id);
        }

        private async Task<ResultT<Guid>> SendFriendshipInviteAsync(UserAccount senderUser, UserAccount recipientUser)
        {
            try
            {
                _logger.LogInformation("SendFriendshipInviteAsync: Creating invitation from {SenderId} to {RecipientId}", senderUser.Id, recipientUser.Id);

                var friendshipInvitation = new FriendshipInvitation
                {
                    SenderId = senderUser.Id,
                    RecipientId = recipientUser.Id,
                    SenderUser = senderUser,
                    RecipientUser = recipientUser
                };

                await _unitOfWork.FriendshipInvitations.AddAsync(friendshipInvitation);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("SendFriendshipInviteAsync: Invitation created successfully with Id {InvitationId}", friendshipInvitation.Id);

                return friendshipInvitation.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendFriendshipInviteAsync: Internal server error for senderId {SenderId} and recipientId {RecipientId}", senderUser.Id, recipientUser.Id);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        private async Task<List<FriendshipInvitationDto>> GetUserInvitationsAsync(UserAccount user)
        {
            _logger.LogInformation("GetUserInvitationsAsync called for userId: {UserId}", user.Id);

            var result = await _unitOfWork.FriendshipInvitations.WhereAsync(
                    x => x.RecipientId == user.Id,
                    x => x.SenderUser,
                    x => x.RecipientUser
                );

            return result.Select(x => new FriendshipInvitationDto()
            {
                FriendInvitationId = x.Id,
                SenderId = x.SenderId,
                SenderUserName = x.SenderUser.UserName ?? throw new Exception(),
                SenderAvatarUrl = x.SenderUser.AvatarUrl != null ? _userAvatarService.GetPublicAvatarUrl(x.SenderUser.AvatarUrl) : null
            }).ToList();
        }

        private async Task<List<UserToInviteDto>> GetUsersToInviteByTextAsync(
            UserAccount user,
            string text)
        {
            _logger.LogInformation("GetUsersToInviteByTextAsync called for userId: {UserId} with text: {Text}", user.Id, text);

            var users = await _userManager.Users
                .AsNoTracking()
                .Where(x =>
                    x.UserName != user.UserName &&
                    x.UserName!.Contains(text))
                .ToListAsync();

            var friends = await GetFriendsFromDbAsync(user.Id);

            var toInvite = friends.Count > 0
                ? users.Where(x => !friends.Any(f => f.Id == x.Id)).ToList()
                : users;

            var dtos = new List<UserToInviteDto>(toInvite.Count);
            foreach (var u in toInvite)
            {
                bool alreadyInvited = await _unitOfWork
                    .FriendshipInvitations
                    .AnyAsync(f =>
                        f.SenderId == u.Id ||
                        f.RecipientId == u.Id);

                dtos.Add(new UserToInviteDto
                {
                    Id = u.Id,
                    UserName = u.UserName!,
                    AvatarUrl = u.AvatarUrl != null ? _userAvatarService.GetPublicAvatarUrl(u.AvatarUrl) : null,
                    IsInvited = alreadyInvited
                });
            }

            _logger.LogInformation("GetUsersToInviteByTextAsync: Found {Count} users to invite", dtos.Count);
            return dtos;
        }

        private async Task<Result> DeleteInviteAsync(UserAccount senderUser, UserAccount recipientUser)
        {
            _logger.LogInformation("DeleteInviteAsync called for senderId: {SenderId}, recipientId: {RecipientId}", senderUser.Id, recipientUser.Id);

            var invitation = await _unitOfWork.FriendshipInvitations
                .FirstOrDefaultAsync(x => x.SenderId == senderUser.Id && x.RecipientId == recipientUser.Id);

            if (invitation != null)
            {
                _unitOfWork.FriendshipInvitations.Delete(invitation);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("DeleteInviteAsync: Invitation deleted successfully");
                return Result.Success();
            }
            else
            {
                _logger.LogWarning("DeleteInviteAsync: Invitation not found for senderId: {SenderId}, recipientId: {RecipientId}", senderUser.Id, recipientUser.Id);
                return Error.NotFound("INVITATION_NOT_FOUND", "Invitation not found.");
            }
        }

        private async Task<Guid> AddFriendsAsync(UserAccount user1, UserAccount user2)
        {
            _logger.LogInformation("AddFriendsAsync called for user1Id: {User1Id}, user2Id: {User2Id}", user1.Id, user2.Id);

            var friendship = new Friendship
            {
                User1Id = user1.Id,
                User2Id = user2.Id,
                User1 = user1,
                User2 = user2
            };

            await _unitOfWork.Friendships.AddAsync(friendship);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation("AddFriendsAsync: Friendship added successfully");

            return friendship.Id;
        }

        private async Task<List<SimpleUserWithAvatarDto>> GetFriendsFromDbAsync(Guid userId)
        {
            _logger.LogInformation("GetFriendsFromDbAsync called for userId: {UserId}", userId);

            var result = await _unitOfWork.Friendships.WhereAsync(
                x => x.User1Id == userId || x.User2Id == userId,
                x => x.User1,
                x => x.User2
            );

            var friends = result.Select(x =>
            {
                if (x.User1Id == userId)
                    return new SimpleUserWithAvatarDto
                    {
                        Id = x.User2Id,
                        userName = x.User2?.UserName ?? string.Empty,
                        AvatarUrl = x.User2?.AvatarUrl != null ? _userAvatarService.GetPublicAvatarUrl(x.User2.AvatarUrl) : null
                    };
                else
                    return new SimpleUserWithAvatarDto
                    {
                        Id = x.User1Id,
                        userName = x.User1?.UserName ?? string.Empty,
                        AvatarUrl = x.User1?.AvatarUrl != null ? _userAvatarService.GetPublicAvatarUrl(x.User1.AvatarUrl) : null
                    };
            }).ToList();

            _logger.LogInformation("GetFriendsFromDbAsync: Found {Count} friends for userId {UserId}", friends.Count, userId);

            return friends;
        }
    }
}

