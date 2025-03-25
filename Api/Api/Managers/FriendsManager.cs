using Api.Data.UnitOfWork;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Friendship;
using Api.Utilities.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Api.Managers
{
    public class FriendsManager : IFriendsManager
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public FriendsManager(UserManager<UserAccount> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> SendInviteAsync(string senderId, string recipientId)
        {
            if (string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(recipientId))
            {
                return Error.BadRequest("SENDERID_IS_EMPTY", "SenderId or RecipientId cannot be null or empty.");
            }

            if (senderId == recipientId)
            {
                return Error.BadRequest("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "Sender ID and Recipient ID must be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId);

                if (senderUser == null || senderUser.UserName == null)
                {
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser was not found.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId);

                if (recipientUser == null || recipientUser.UserName == null)
                {
                    return Error.NotFound("RECIPIENTUSER_NOT_FOUND", "RecipientUser was not found.");
                }

                if (await IsFriendsInvitationExists(senderId, recipientId))
                {
                    return Error.Conflict("FRIENDS_INVITATION_EXISTS", "An invitation has already exist.");
                }

                await SendFriendshipInviteAsync(senderUser, recipientUser);

                return Result.Success();
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<SimpleUserDto>>> GetInvitationsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Error.BadRequest("USERID_IS_EMPTY", "UserId cannot be null or empty.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null || user.UserName == null)
                {
                    return Error.NotFound("USERID_NOT_FOUND", "User was not found.");
                }

                var list = await GetUserInvitationsAsync(user);

                if (list.Count > 0)
                {
                    return list;
                }

                return Error.NotFound("INVITITIES_NOT_FOUND", "Invitations were not found.");

            }
            catch
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<UserToInviteDto>>> GetUsersToInviteByTextAsync(string userId, string text)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Error.BadRequest("USERID_IS_EMPTY", "UserId cannot be null or empty.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null || user.UserName == null)
                {
                    return Error.NotFound("USERID_NOT_FOUND", "User was not found.");
                }

                var list = await GetUsersToInviteByTextAsync(user, text);

                if (list.Count > 0)
                {
                    return list;
                }

                return Error.NotFound("INVITITIES_NOT_FOUND", "Users to invite were not found.");

            }
            catch
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<Result> DecelineInviteAsync(string senderId, string recipientId)
        {
            if (string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(recipientId))
            {
                return Error.BadRequest("SENDERID_IS_EMPTY", "SenderId and RecipientId cannot be null or empty.");
            }

            if (senderId == recipientId)
            {
                return Error.BadRequest("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "SenderId and RecipientId must be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId);

                if (senderUser == null || senderUser.UserName == null)
                {
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser was not found.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId);

                if (recipientUser == null || recipientUser.UserName == null)
                {
                    return Error.NotFound("RECIPIENTUSER_NOT_FOUND", "RecipientUser was not found.");
                }

                var result = await DeleteInviteAsync(senderUser, recipientUser);

                if(result.IsSuccess)
                {
                    return Result.Success();
                } 
                
                if(result.Error != null)
                {
                    return Error.NotFound("INVITATION_NOT_FOUND", "Invitation was not found.");
                }

                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<Result> AddFriendsAsync(string senderId, string recipientId)
        {
            if (string.IsNullOrWhiteSpace(senderId) || string.IsNullOrWhiteSpace(recipientId))
            {
                return Error.BadRequest("SENDERID_IS_EMPTY", "SenderId and RecipientId cannot be null or empty.");
            }

            if (senderId == recipientId)
            {
                return Error.BadRequest("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "Sender ID and Recipient ID must be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId);

                if (senderUser == null || senderUser.UserName == null)
                {
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser was not found.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId);

                if (recipientUser == null || recipientUser.UserName == null)
                {
                    return Error.NotFound("RECIPIENTUSER_NOT_FOUND", "RecipientUser was not found.");
                }

                if (await IsFriendsInvitationExists(senderId, recipientId) == false)
                {
                    return Error.NotFound("INVITATION_NOT_FOUND", "The invitation was not found.");
                }

                if(await IsFriendsExistAsync(senderId, recipientId))
                {
                    return Error.Conflict("FRIENDS_ALREADY_EXISTS", "This relationship has already exist.");
                }

                await DeleteInviteAsync(senderUser, recipientUser);
                await AddFriendsAsync(senderUser, recipientUser);

                return Result.Success();
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<SimpleUserDto>>> GetFriendsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Error.BadRequest("USERID_IS_EMPTY", "UserId cannot be null or empty.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null || user.UserName == null)
                {
                    return Error.NotFound("USERID_NOT_FOUND", "User was not found.");
                }

                var list = await GetFriendsFromDbAsync(userId);

                if (list.Count > 0)
                {
                    return list;
                }

                return Error.NotFound("INVITITIES_NOT_FOUND", "No friends were found.");

            }
            catch
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<bool> IsFriendsExistAsync(string userId1, string userId2)
        {
            return await _unitOfWork.Friendships
                .AnyAsync(x => (x.User1Id == userId1 && x.User2Id == userId2)
                || (x.User1Id == userId2 && x.User2Id == userId1));
        }

        private async Task<bool> IsFriendsInvitationExists(string user1Id, string user2Id)
        {
            return await _unitOfWork.FriendshipInvitations.AnyAsync(x =>
            (x.SenderId == user1Id && x.RecipientId == user2Id) ||
            (x.SenderId == user2Id && x.RecipientId == user1Id));
        }

        private async Task SendFriendshipInviteAsync(UserAccount senderUser, UserAccount recipientUser)
        {
            var friendshipInvitation = new FriendshipInvitation
            {
                SenderId = senderUser.Id,
                RecipientId = recipientUser.Id,
                SenderUser = senderUser,
                RecipientUser = recipientUser
            };

            await _unitOfWork.FriendshipInvitations.AddAsync(friendshipInvitation);
            await _unitOfWork.SaveAsync();
        }

        private async Task<List<SimpleUserDto>> GetUserInvitationsAsync(UserAccount user)
        {
            var result = await _unitOfWork.FriendshipInvitations.WhereAsync(
                    x => x.RecipientId == user.Id,
                    x => x.SenderUser,
                    x => x.RecipientUser
                );

            return result.Select(x => new SimpleUserDto()
            {
                Id = x.SenderId,
                userName = x.SenderUser.UserName ?? throw new Exception()
            }).ToList();
        }

        private async Task<List<UserToInviteDto>> GetUsersToInviteByTextAsync(UserAccount user, string text)
        {
            var users = await _userManager.Users
                .Where(x => x.UserName != user.UserName && x.UserName!.Contains(text))
                .ToListAsync();

            var invitations = _unitOfWork.FriendshipInvitations;

            var result = new List<UserToInviteDto>();

            foreach (var x in users)
            {
                var isInvited = await invitations.AnyAsync(inv =>
                    (inv.SenderId == user.Id && inv.RecipientId == x.Id) ||
                    (inv.RecipientId == user.Id && inv.SenderId == x.Id));

                result.Add(new UserToInviteDto
                {
                    Id = x.Id,
                    userName = x.UserName ?? throw new Exception("UserName is null"),
                    IsInvited = isInvited
                });
            }

            return result;
        }

        private async Task<Result> DeleteInviteAsync(UserAccount senderUser, UserAccount recipientUser)
        {
            var invitation = await _unitOfWork.FriendshipInvitations
                .FirstOrDefaultAsync(x => x.SenderId == senderUser.Id && x.RecipientId == recipientUser.Id);

            if (invitation != null)
            {
                _unitOfWork.FriendshipInvitations.Delete(invitation);
                await _unitOfWork.SaveAsync();
                return Result.Success();
            }
            else
            {
                return Error.NotFound("INVITATION_NOT_FOUND", "Invitation not found.");
            }
        }

        private async Task AddFriendsAsync(UserAccount user1, UserAccount user2)
        {
            var friendship = new Friendship
            {
                User1Id = user1.Id,
                User2Id = user2.Id,
                User1 = user1,
                User2 = user2
            };

            await _unitOfWork.Friendships.AddAsync(friendship);
            await _unitOfWork.SaveAsync();
        }

        private async Task<List<SimpleUserDto>> GetFriendsFromDbAsync(string userId)
        {
            var result = await _unitOfWork.Friendships.WhereAsync(
                x => (x.User1Id == userId || x.User2Id == userId),
                x => x.User1,
                x => x.User2);

            return result.Select(x => new SimpleUserDto()
            {
                Id = x.User1Id == userId ? x.User2Id : x.User1Id,
                userName = x.User1Id == userId ? (x.User2.UserName ?? throw new Exception()) : (x.User1.UserName ?? throw new Exception())
            }).ToList();
        }

    }
}
