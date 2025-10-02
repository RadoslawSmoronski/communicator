using Application.Interfaces.Users;
using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Infrastructure.Services
{
    public class FriendInvitationsService : IFriendInvitationsService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FriendInvitationsService> _logger;

        public FriendInvitationsService(UserManager<UserAccount> userManager, IUnitOfWork unitOfWork, ILogger<FriendInvitationsService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Guid>> SendInviteAsync(Guid senderId, Guid recipientId)
        {
            if (senderId == recipientId)
            {
                _logger.LogWarning("Sender ID and Recipient ID are the same: {SenderId}", senderId);
                return Error.Validation("FriendInvitation.SameUser", "Sender and recipient cannot be the same user.");
            }

            try
            {
                var senderUser = await _userManager.FindByIdAsync(senderId.ToString());

                if (senderUser is null || senderUser.UserName is null)
                {
                    _logger.LogWarning("Sender user not found or username is null. SenderId: {SenderId}", senderId);
                    return Error.NotFound("FriendInvitation.SenderNotFound", "Sender user was not found.");
                }

                var recipientUser = await _userManager.FindByIdAsync(recipientId.ToString());

                if (recipientUser is null || recipientUser.UserName is null)
                {
                    _logger.LogWarning("Recipient user not found or username is null. RecipientId: {RecipientId}", recipientId);
                    return Error.NotFound("FriendInvitation.RecipientNotFound", "Recipient user was not found.");
                }

                if (recipientUser.EmailConfirmed is false)
                {
                    _logger.LogWarning("Recipient user's email is not confirmed. RecipientId: {RecipientId}", recipientId);
                    return Error.Validation("FriendInvitation.RecipientEmailNotConfirmed", "Recipient user's email is not confirmed.");
                }

                // refactor: Check if friendship already exists


                if (await IsFriendInvitationExistsAsync(senderUser.Id, recipientUser.Id))
                {
                    _logger.LogWarning("Friend invitation already exists between users {SenderId} and {RecipientId}", senderUser.Id, recipientUser.Id);
                    return Error.Conflict("FriendInvitation.AlreadyExists", "A friend invitation already exists between these users.");
                }

                var friendshipInvitation = new FriendshipInvitation
                {
                    SenderId = senderUser.Id,
                    RecipientId = recipientUser.Id
                };

                await _unitOfWork.FriendshipInvitations.AddAsync(friendshipInvitation);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Friend invitation sent from {SenderId} to {RecipientId}. InvitationId: {InvitationId}", senderUser.Id, recipientUser.Id, friendshipInvitation.Id);

                return friendshipInvitation.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending a friend invitation from {SenderId} to {RecipientId}", senderId, recipientId);
                return Error.Failure("FriendInvitation.Failure", "An unexpected error occurred while sending the friend invitation.");
            }
        }

        private async Task<bool> IsFriendInvitationExistsAsync(Guid user1Id, Guid user2Id)
        {
            return await _unitOfWork.FriendshipInvitations.AnyAsync(x =>
                x.SenderId == user1Id && x.RecipientId == user2Id ||
                x.SenderId == user2Id && x.RecipientId == user1Id);
        }
    }
}
