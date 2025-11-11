using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using Application.Repositories;
using Application.Users.Queries.GetInvitations;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Infrastructure.Services
{
    public class FriendInvitationsService : IFriendInvitationsService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FriendInvitationsService> _logger;
        private readonly IUserAvatarService _userAvatarService;
        private readonly ICurrentUser _user;

        public FriendInvitationsService(UserManager<UserAccount> userManager, IUnitOfWork unitOfWork, ILogger<FriendInvitationsService> logger, ICurrentUser user, IUserAvatarService userAvatarService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _user = user;
            _userAvatarService = userAvatarService;
        }

        public async Task<Result<Guid>> SendAsync(Guid senderId, Guid recipientId)
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

                if (await _unitOfWork.FriendshipInvitations.IsExistAsync(senderId, recipientId))
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

        public async Task<Result> DeleteAsync(Guid invitationId) => await _AcceptDeleteInviteAsync(invitationId, false);

        public async Task<Result<FriendshipInvitation>> AcceptAsync(Guid invitationId) => await _AcceptDeleteInviteAsync(invitationId, true);

        private async Task<Result<FriendshipInvitation>> _AcceptDeleteInviteAsync(Guid InvitationId, bool IsAcceptInvitation)
        {
            var loggerTag = IsAcceptInvitation ? "Accept" : "Delete";

            try
            {
                var invitation = await _unitOfWork.FriendshipInvitations.GetById(InvitationId);

                if (invitation is null)
                {
                    _logger.LogWarning("Friend invitation not found. InvitationId: {InvitationId}", InvitationId);
                    return Error.NotFound($"{loggerTag}InviteAsync.NotFound", "Friend invitation was not found.");
                }

                await _unitOfWork.FriendshipInvitations.DeleteAsync(invitation.Id);
                await _unitOfWork.SaveAsync();

                _logger.LogInformation("Friend invitation {Action}d. InvitationId: {InvitationId}, UserId: {UserId}", loggerTag.ToLower(), InvitationId, _user.Id);
                return invitation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while {Action}ing friend invitation. InvitationId: {InvitationId}, UserId: {UserId}", loggerTag.ToLower(), InvitationId, _user.Id);
                return Error.Failure($"AcceptFriendInvitation.Failure", $"An unexpected error occurred while {loggerTag.ToLower()}ing the friend invitation.");
            }
        }

        public async Task<Result<List<GetInvitationsReadModel>>> GetInvitationsSendedToUserAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.UserName == null)
                {
                    _logger.LogWarning("User not found or username is null. UserId: {UserId}", userId);
                    return Error.NotFound("FriendInvitation.UserNotFound", "User was not found.");
                }

                var invitations = await _unitOfWork.FriendshipInvitations.GetAllAsync(userId);

                var senderIds = invitations.Where(x=> x.RecipientId == userId).Select(x => x.SenderId).Distinct().ToList();

                var senders = await _userManager.Users
                    .Where(u => senderIds.Contains(u.Id))
                    .ToListAsync();

                var senderDict = senders.ToDictionary(u => u.Id);

                var dtos = invitations.Select(x =>
                {
                    var senderUser = senderDict.TryGetValue(x.SenderId, out var userAcc) ? userAcc : null;
                    return new GetInvitationsReadModel(FriendInvitationId: x.Id,
                        SenderId: x.SenderId,
                        SenderUserName: senderUser?.UserName ?? string.Empty,
                        SenderAvatarUrl: senderUser?.AvatarUrl != null ? _userAvatarService.GetPublicAvatarUrl(senderUser.AvatarUrl) : null
                        );
                }).ToList();

                _logger.LogInformation("Retrieved {Count} friend invitations for user {UserId}", dtos.Count, userId);

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving friend invitations for user {UserId}", userId);
                return Error.Failure("FriendInvitation.GetFailure", "An unexpected error occurred while retrieving friend invitations.");
            }
        }

        public async Task<Result<List<FriendshipInvitation>>> GetUserInvitations(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("GetUserInvitations: empty userId provided.");
                return Error.Validation("FriendInvitation.InvalidUserId", "User ID cannot be empty.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null || user.UserName == null)
                {
                    _logger.LogWarning("GetUserInvitations: user not found or username is null. UserId: {UserId}", userId);
                    return Error.NotFound("FriendInvitation.UserNotFound", "User was not found.");
                }

                var invitations = await _unitOfWork.FriendshipInvitations.GetAllAsync(userId);

                _logger.LogInformation("GetUserInvitations: retrieved {InvitationCount} friend invitations for user {UserId}", invitations.Count, userId);

                return invitations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserInvitations: unexpected error while retrieving friend invitations for user {UserId}", userId);
                return Error.Failure("FriendInvitation.GetUserInvitations.Failure", "An unexpected error occurred while retrieving friend invitations.");
            }
        }
    }
}
