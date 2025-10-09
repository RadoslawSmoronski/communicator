using Application.Common.Interfaces;
using Application.DTOs;
using Application.Interfaces;
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
        private readonly IUser _user;

        public FriendInvitationsService(UserManager<UserAccount> userManager, IUnitOfWork unitOfWork, ILogger<FriendInvitationsService> logger, IUser user)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _user = user;
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

        public async Task<Result<FriendshipInviteOperationDto>> DeleteInviteAsync(Guid invitationId) => await _AcceptDeleteInviteAsync(invitationId, false);

        public async Task<Result<FriendshipInviteOperationDto>> AcceptInviteAsync(Guid invitationId) => await _AcceptDeleteInviteAsync(invitationId, true);

        private async Task<Result<FriendshipInviteOperationDto>> _AcceptDeleteInviteAsync(Guid InvitationId, bool IsAcceptInvitation)
        {
            var loggerTag = IsAcceptInvitation ? "Accept" : "Delete";

            try
            {
                var invitation = await _unitOfWork.FriendshipInvitations.FirstOrDefaultAsync(x => x.Id == InvitationId);

                if (invitation is null)
                {
                    _logger.LogWarning("Friend invitation not found. InvitationId: {InvitationId}", InvitationId);
                    return Error.NotFound($"{loggerTag}InviteAsync.NotFound", "Friend invitation was not found.");
                }

                if (IsAcceptInvitation && IsAuthorizedToAcceptInvitation(invitation) is false)
                {
                    _logger.LogWarning("Unauthorized attempt to accept invitation. InvitationId: {InvitationId}, UserId: {UserId}", InvitationId, _user.Id);
                    return Error.Unauthorized("AcceptFriendInvitation.Unauthorized", "You are not authorized to accept this friend invitation.");
                }

                if (!IsAcceptInvitation && IsAuthorizedToDeleteInvitation(invitation) is false)
                {
                    _logger.LogWarning("Unauthorized attempt to delete invitation. InvitationId: {InvitationId}, UserId: {UserId}", InvitationId, _user.Id);
                    return Error.Unauthorized("DeleteFriendInvitation.Unauthorized", "You are not authorized to delete this friend invitation.");
                }

                _unitOfWork.FriendshipInvitations.Delete(invitation);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Friend invitation {Action}d. InvitationId: {InvitationId}, UserId: {UserId}", loggerTag.ToLower(), InvitationId, _user.Id);
                return new FriendshipInviteOperationDto()
                {
                    SenderId = invitation.SenderId,
                    RecipientId = invitation.RecipientId,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while {Action}ing friend invitation. InvitationId: {InvitationId}, UserId: {UserId}", loggerTag.ToLower(), InvitationId, _user.Id);
                return Error.Failure($"AcceptFriendInvitation.Failure", $"An unexpected error occurred while {loggerTag.ToLower()}ing the friend invitation.");
            }
        }

        private async Task<bool> IsFriendInvitationExistsAsync(Guid user1Id, Guid user2Id)
        {
            return await _unitOfWork.FriendshipInvitations.AnyAsync(x =>
                x.SenderId == user1Id && x.RecipientId == user2Id ||
                x.SenderId == user2Id && x.RecipientId == user1Id);
        }

        private bool IsAuthorizedToDeleteInvitation(FriendshipInvitation friendshipInvitation)
        {
            var isAdmin = _user.Roles?.Contains("Admin") ?? false;

            if (isAdmin)
                return true;

            if (friendshipInvitation.SenderId == _user.Id || friendshipInvitation.RecipientId == _user.Id)
            {
                return true;
            }

            return false;
        }

        private bool IsAuthorizedToAcceptInvitation(FriendshipInvitation friendshipInvitation)
        {
            var isAdmin = _user.Roles?.Contains("Admin") ?? false;

            if (isAdmin)
                return true;

            return friendshipInvitation.RecipientId == _user.Id;
        }
    }
}
