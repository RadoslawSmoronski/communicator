using Api.Data.IRepository;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Utilities.Result;
using Microsoft.AspNetCore.Hosting.Server;
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

        public async Task<Result> SendInviteAsync(string SenderId, string RecipientId)
        {
            if (string.IsNullOrWhiteSpace(SenderId))
            {
                return Error.BadRequest("SENDERID_IS_EMPTY", "SenderId is required.");
            }

            if (string.IsNullOrWhiteSpace(RecipientId))
            {
                return Error.BadRequest("RECIPIENTID_IS_EMPTY", "RecipientId is required.");
            }

            if(SenderId == RecipientId)
            {
                return Error.BadRequest("SENDERID_AND_RECIPIENTID_ARE_THE_SAME", "SenderId and RecipientId need to be different.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(SenderId);

                if (senderUser == null || senderUser.UserName == null)
                {
                    return Error.NotFound("SENDERUSER_NOT_FOUND", "SenderUser doesn't exist.");
                }

                var recipientUser = await _userManager.FindByIdAsync(RecipientId);

                if (recipientUser == null || recipientUser.UserName == null)
                {
                    return Error.NotFound("RECIPIENTUSER_NOT_FOUND", "RecipientUser doesn't exist.");
                }

                if(await _friendsRepository.IsFriendsInvitationExists(SenderId, RecipientId))
                {
                    return Error.Conflict("FRIENDS_INVITATION_EXISTS", "An invitation has already exist.");
                }

                await _friendsRepository.SendInviteAsync(recipientUser, senderUser);

                return Result.Success();
            }
            catch(Exception)
            {
                return Error.InternalServerError("INTERNAL_SERVER_ERROR", "An internal server error occurred.");
            }
        }
    }
}
