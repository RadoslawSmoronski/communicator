using Api.Data.IRepository;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Utilities.Result;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Api.Managers
{
    public class FriendsManager : IFriendsManager
    {
        private readonly IFriendsRepository _friendsRepository;
        private readonly UserManager<UserAccount> _userManager;

        public FriendsManager(IFriendsRepository friendsRepository, UserManager<UserAccount> userManager)
        {
            _friendsRepository = friendsRepository;
            _userManager = userManager;
        }

        public async Task<Result> SendInviteAsync(string senderId, string recipientId)
        {
            if (string.IsNullOrWhiteSpace(senderId))
            {
                return Error.BadRequest("SENDERID_IS_EMPTY", "SenderId is required.");
            }

            if (string.IsNullOrWhiteSpace(recipientId))
            {
                return Error.BadRequest("RECIPIENTID_IS_EMPTY", "RecipientId is required.");
            }

            if (senderId == recipientId)
            {
                return Error.BadRequest("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "SenderId and RecipientId need to be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId);

                if (senderUser == null || senderUser.UserName == null)
                {
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser doesn't exist.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId);

                if (recipientUser == null || recipientUser.UserName == null)
                {
                    return Error.NotFound("RECIPIENTUSER_NOT_FOUND", "RecipientUser doesn't exist.");
                }

                if (await _friendsRepository.IsFriendsInvitationExists(senderId, recipientId))
                {
                    return Error.Conflict("FRIENDS_INVITATION_EXISTS", "An invitation has already exist.");
                }

                await _friendsRepository.SendInviteAsync(senderUser, recipientUser);

                return Result.Success();
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<ResultT<List<GetInvitationsUserDto>>> GetInvitationsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Error.BadRequest("USERID_IS_EMPTY", "UserId is required.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null || user.UserName == null)
                {
                    return Error.NotFound("USERID_NOT_FOUND", "User doesn't exist.");
                }

                var list = await _friendsRepository.GetInvitationsAsync(userId);

                if(list.Count > 0)
                {
                    return list;
                }

                return Error.NotFound("INVITITIES_NOT_FOUND", "No invitation found.");

            }
            catch
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }

        public async Task<Result> DeleteInviteAsync(string senderId, string recipientId)
        {
            if (string.IsNullOrWhiteSpace(senderId))
            {
                return Error.BadRequest("SENDERID_IS_EMPTY", "SenderId is required.");
            }

            if (string.IsNullOrWhiteSpace(recipientId))
            {
                return Error.BadRequest("RECIPIENTID_IS_EMPTY", "RecipientId is required.");
            }

            if (senderId == recipientId)
            {
                return Error.BadRequest("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "SenderId and RecipientId need to be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId);

                if (senderUser == null || senderUser.UserName == null)
                {
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser doesn't exist.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId);

                if (recipientUser == null || recipientUser.UserName == null)
                {
                    return Error.NotFound("RECIPIENTUSER_NOT_FOUND", "RecipientUser doesn't exist.");
                }

                if (await _friendsRepository.IsFriendsInvitationExists(senderId, recipientId) == false)
                {
                    return Error.NotFound("INVITATION_NOT_FOUND", "Invitation not found.");
                }

                var result = await _friendsRepository.DeleteInviteAsync(senderUser, recipientUser);

                if(result.IsSuccess)
                {
                    return Result.Success();
                } 
                
                if(result.Error != null)
                {
                    return Error.NotFound("INVITATION_NOT_FOUND", "Invitation not found.");
                }

                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
            catch (Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }
    }
}
