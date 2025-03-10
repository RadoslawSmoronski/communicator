using Api.Data.IRepository;
using Api.Data.UnitOfWork;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Friendship;
using Api.Utilities.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Managers
{
    public class FriendsManager : IFriendsManager
    {
        private readonly IFriendsRepository _friendsRepository;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public FriendsManager(IFriendsRepository friendsRepository, UserManager<UserAccount> userManager, IUnitOfWork unitOfWork)
        {
            _friendsRepository = friendsRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> SendInviteAsync(string senderId, string recipientId)
        {
            if (string.IsNullOrWhiteSpace(senderId) && string.IsNullOrWhiteSpace(recipientId))
            {
                return Error.BadRequest("SENDERID_IS_EMPTY", "SenderId and RecipientId cannot be null or empty");
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

        public async Task<ResultT<List<FriendDto>>> GetInvitationsAsync(string userId)
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
                    return list.Select(x => new FriendDto
                    {
                        Id = x.SenderId,
                        UserName = "to do, in progess.."
                    }).ToList();
                }

                return Error.NotFound("INVITITIES_NOT_FOUND", "No invitation was found.");

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
                return Error.BadRequest("SENDERID_IS_EMPTY", "SenderId or RecipientId cannot be null or empty.");
            }

            if (senderId == recipientId)
            {
                return Error.BadRequest("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "SenderID and RecipientID must be different.");
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

                if(await IsFriendsExists(senderId, recipientId))
                {
                    return Error.Conflict("FRIENDS_ALREADY_EXISTS", "This relationship already exists.");
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

        public async Task<ResultT<List<FriendDto>>> GetFriendsAsync(string userId)
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

        private async Task<List<FriendshipInvitation>> GetUserInvitationsAsync(UserAccount user)
        {
            var result = await _unitOfWork.FriendshipInvitations
                .WhereAsync(x => x.RecipientId == user.Id);
            
            return result.ToList();
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

        private async Task<bool> IsFriendsExists(string userId1, string userId2)
        {
            return await _unitOfWork.Friendships
                .AnyAsync(x => (x.User1Id == userId1 && x.User2Id == userId2)
                || (x.User1Id == userId2 && x.User2Id == userId1));
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

        private async Task<List<FriendDto>> GetFriendsFromDbAsync(string userId)
        {
            var result = await _unitOfWork.Friendships.WhereAsync(x => x.User1Id == userId || x.User2Id == userId);

            return result.Select(x => new FriendDto()
            {
                Id = x.User1Id == userId ? x.User2Id : x.User1Id,
                UserName = x.User1Id == userId ? x.User2.UserName! : x.User1.UserName!
            }).ToList();
        }

    }
}
