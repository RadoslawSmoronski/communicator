using Api.Data.UnitOfWork;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Friendship;
using Api.Utilities.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Result> SendInviteAsync(Guid senderId, Guid recipientId)
        {
            if (senderId == Guid.Empty || recipientId == Guid.Empty)
            {
                return Error.Validation("SENDERID_IS_EMPTY", "SenderId or RecipientId cannot be empty.");
            }

            if (senderId == recipientId)
            {
                return Error.Validation("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "Sender ID and Recipient ID must be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId.ToString());

                if (senderUser == null || senderUser.UserName == null)
                {
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser was not found.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId.ToString());

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
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }
            
        public async Task<ResultT<List<SimpleUserDto>>> GetInvitationsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return Error.Validation("USERID_IS_EMPTY", "UserId cannot be null or empty.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.UserName == null)
                {
                    return Error.NotFound("USERID_NOT_FOUND", "User was not found.");
                }

                var list = await GetUserInvitationsAsync(user);

                return list;
            }
            catch
            {
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<UserToInviteDto>>> GetUsersToInviteByTextAsync(Guid userId, string text)
        {
            if ( userId == Guid.Empty || string.IsNullOrWhiteSpace(text))
            {
                return Error.Validation("USERID_OR_TEXT_ARE_EMPTY", "UserId or text cannot be null or empty.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.UserName == null)
                {
                    return Error.NotFound("USERID_NOT_FOUND", "User was not found.");
                }

                var list = await GetUsersToInviteByTextAsync(user, text);

                return list;
            }
            catch
            {
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<Result> DecelineInviteAsync(Guid senderId, Guid recipientId)
        {
            if ( senderId == Guid.Empty || recipientId == Guid.Empty)
            {
                return Error.Validation("SENDERID_IS_EMPTY", "SenderId and RecipientId cannot be empty.");
            }

            if (senderId == recipientId)
            {
                return Error.Validation("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "SenderId and RecipientId must be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId.ToString());

                if (senderUser == null || senderUser.UserName == null)
                {
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser was not found.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId.ToString());

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

                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
            catch (Exception)
            {
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<Result> AddFriendsAsync(Guid senderId, Guid recipientId)
        {
            if (senderId == Guid.Empty || recipientId == Guid.Empty)
            {
                return Error.Validation("SENDERID_IS_EMPTY", "SenderId and RecipientId cannot be null or empty.");
            }

            if (senderId == recipientId)
            {
                return Error.Validation("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "Sender ID and Recipient ID must be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId.ToString());

                if (senderUser == null || senderUser.UserName == null)
                {
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser was not found.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId.ToString());

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
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<SimpleUserDto>>> GetFriendsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return Error.Validation("USERID_IS_EMPTY", "UserId cannot be null or empty.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null || user.UserName == null)
                {
                    return Error.NotFound("USERID_NOT_FOUND", "User was not found.");
                }

                var list = await GetFriendsFromDbAsync(userId);

                return list;
            }
            catch
            {
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<bool> IsFriendsExistAsync(Guid userId1, Guid userId2)
        {
            return await _unitOfWork.Friendships
                .AnyAsync(x => (x.User1Id == userId1 && x.User2Id == userId2)
                || (x.User1Id == userId2 && x.User2Id == userId1));
        }

        private async Task<bool> IsFriendsInvitationExists(Guid user1Id, Guid user2Id)
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
                .AsNoTracking()
                .Where(x => x.UserName != user.UserName && x.UserName!.Contains(text))
                .ToListAsync();

            var result = new List<UserToInviteDto>();

            foreach (var x in users)
            {
                var isInvited = await _unitOfWork.FriendshipInvitations.AnyAsync(inv =>
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

        private async Task<List<SimpleUserDto>> GetFriendsFromDbAsync(Guid userId)
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
