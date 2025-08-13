using ChatCommunicator.Contracts.Dtos.Controllers.FriendsController;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.API.Controllers;
using ChatCommunicator.Contracts.Dtos.Friendships;

namespace ChatCommunicator.Application.Controllers
{
    [Route("api/friend-invitations")]
    [ApiController]
    public class FriendInvitationsController : BaseController
    {
        private readonly IFriendsService _friendsManager;
        private readonly IChatFriendsService _chatFriendsService;
        private readonly ILogger<FriendInvitationsController> _logger;

        public FriendInvitationsController(IFriendsService friendsManager,
            IChatFriendsService chatFriendsService,
            ILogger<FriendInvitationsController> logger)
        {
            _chatFriendsService = chatFriendsService;
            _friendsManager = friendsManager;
            _logger = logger;
        }

        /// <summary>
        /// Send Invite
        /// </summary>
        /// <remarks>
        /// Allows an authenticated user to send a friend invitation by providing the sender's and recipient's GUIDs.
        /// Returns a <see cref="SendInviteDto"/> object containing the invitation ID on success, or an error response on failure.
        /// </remarks>
        /// <param name="inviteDto">Invitation data including sender and recipient GUIDs.</param>
        /// <returns>
        /// <see cref="SendInviteDto"/> with the created invitation ID, or an error response detailing the failure.
        /// </returns>
        /// <response code="201">Invitation sent successfully. Returns <see cref="SendInviteDto"/>.</response>
        /// <response code="400">Invalid invitation data (e.g., malformed or missing GUIDs).</response>
        /// <response code="401">Unauthorized - JWT token required.</response>
        /// <response code="404">Recipient user not found.</response>
        /// <response code="409">Conflict (e.g., invitation already exists).</response>
        /// <response code="500">Unexpected server error.</response>
        /// <example>
        /// <code>
        /// POST /api/friends/friend-invitations
        /// {
        ///     "senderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "recipientId": "d2719f9d-8f8c-4b5a-80c4-07afcf1c5b90"
        /// }
        /// 
        /// Response:
        /// {
        ///     "friendshipInvitationId": "e2b1c7e2-4b7a-4f5e-9c2a-1a2b3c4d5e6f"
        /// }
        /// </code>
        /// </example>
        [Authorize]
        [HttpPost()]
        [ProducesResponseType(typeof(SendInviteDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> SendInviteAsync([FromBody] InviteDto inviteDto)
        {
            var inviteResult = await _friendsManager.SendInviteAsync(inviteDto.SenderId, inviteDto.RecipientId);

            if (inviteResult.IsSuccess)
            {
            _logger.LogInformation("[SendInviteAsync] Invite sent successfully. SenderId: {SenderId}, RecipientId: {RecipientId}",
                inviteDto.SenderId, inviteDto.RecipientId);
            return StatusCode(StatusCodes.Status201Created, new SendInviteDto
            {
                FriendshipInvitationId = inviteResult.Value
            });
            }

            return HandleError(inviteResult, "SendInviteAsync", _logger);
        }

        /// <summary>
        /// Decline Invite
        /// </summary>
        /// <remarks>
        /// Allows an authenticated user to decline a friend invitation by providing the invitation ID, sender and recipient GUIDs.
        /// Returns appropriate errors if the invitation is invalid, already handled, or missing.
        /// </remarks>
        /// <param name="friendInvitationId">The unique identifier of the friend invitation to decline.</param>
        /// <param name="inviteDto">Invitation data with sender and recipient GUIDs.</param>
        /// <returns>
        /// HTTP 200 on success or an error describing the failure.
        /// </returns>
        /// <response code="200">Invitation declined successfully.</response>
        /// <response code="400">Invalid data (e.g., malformed GUIDs).</response>
        /// <response code="401">Unauthorized - JWT token required.</response>
        /// <response code="404">Invitation not found or already declined.</response>
        /// <response code="500">Unexpected server error.</response>
        /// <example>
        /// <code>
        /// POST /api/friends/friend-invitations/{friendInvitationId}/decline
        /// {
        ///     "senderId": "b0f4e7d2-115a-4dcf-b9f5-5b08f6bcb034",
        ///     "recipientId": "a54ff5c4-6f67-4c10-b4a7-b6d3f0d8c9cb"
        /// }
        /// </code>
        /// </example>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize]
        [HttpPost("{friendInvitationId}/decline")]
        [HttpPatch("{friendInvitationId}/decline")]
        public async Task<IActionResult> DecelineInviteAsync([FromRoute] Guid friendInvitationId, [FromBody] InviteDto inviteDto)
        {
            var result = await _friendsManager.DecelineInviteAsync(inviteDto.SenderId, inviteDto.RecipientId); //REFACTOR

            if (result.IsSuccess)
            {
                _logger.LogInformation("[DecelineInviteAsync] Invite declined successfully. SenderId: {SenderId}, RecipientId: {RecipientId}",
                    inviteDto.SenderId, inviteDto.RecipientId);
                return Ok();
            }

            return HandleError(result, "DecelineInviteAsync", _logger);
        }


        /// <summary>
        /// Accept Friend Invitation
        /// </summary>
        /// <remarks>
        /// Allows an authenticated user to accept a friend invitation by providing the invitation ID, sender's and recipient's GUIDs.
        /// On success, both users are added to each other's friend lists and a conversation is created between them.
        /// Returns an <see cref="AcceptFriendshipInviteDto"/> object containing the friendship ID and the conversation ID.
        /// Returns errors for invalid input, not found invitations, or conflicts (e.g., already friends).
        /// </remarks>
        /// <param name="friendInvitationId">The unique identifier of the friend invitation to accept.</param>
        /// <param name="acceptInviteDto">Invitation data including sender and recipient GUIDs.</param>
        /// <returns>
        /// <see cref="AcceptFriendshipInviteDto"/> containing the friendship ID and conversation ID on success, or an error response.
        /// </returns>
        /// <response code="201">Invitation accepted; users are now friends and a conversation is created. Returns <see cref="AcceptFriendshipInviteDto"/>.</response>
        /// <response code="400">Invalid data provided (e.g., malformed or missing GUIDs).</response>
        /// <response code="401">Unauthorized - JWT token required.</response>
        /// <response code="404">Invitation not found.</response>
        /// <response code="409">Conflict - users already friends or invitation already accepted.</response>
        /// <response code="500">Unexpected server error.</response>
        /// <example>
        /// <code>
        /// POST /api/friends/friend-invitations/{friendInvitationId}/accept
        /// {
        ///     "senderId": "b0f4e7d2-115a-4dcf-b9f5-5b08f6bcb034",
        ///     "recipientId": "a54ff5c4-6f67-4c10-b4a7-b6d3f0d8c9cb"
        /// }
        /// 
        /// Response:
        /// {
        ///     "friendshipId": "e2b1c7e2-4b7a-4f5e-9c2a-1a2b3c4d5e6f",
        ///     "conversationId": "f3c2d1e4-5b6a-7c8d-9e0f-1a2b3c4d5e6f"
        /// }
        /// </code>
        /// </example>
        [Authorize]
        [HttpPost("{friendInvitationId}/accept")]
        [HttpPatch("{friendInvitationId}/accept")]
        [ProducesResponseType(typeof(AcceptFriendshipInviteDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> AcceptInviteAsync([FromRoute] Guid friendInvitationId, [FromBody] InviteDto acceptInviteDto)
        {
            var result = await _chatFriendsService.AddFriendAndCreateConversationAsync(acceptInviteDto.SenderId, acceptInviteDto.RecipientId); //REFACTOR

            if (result.IsSuccess)
            {
                _logger.LogInformation("[AcceptInviteAsync] Friends added and conversation created successfully. SenderId: {SenderId}, RecipientId: {RecipientId}",
                acceptInviteDto.SenderId, acceptInviteDto.RecipientId);
                return StatusCode(StatusCodes.Status201Created, result.Value);
            }

            return HandleError(result, "AcceptInviteAsync", _logger);
        }
    }
}