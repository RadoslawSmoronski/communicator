using ChatCommunicator.Contracts.Dtos.Controllers.FriendsController;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using ChatCommunicator.Contracts.Dtos;
using System.Security.Claims;
using ChatCommunicator.Shared.Result;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.API.Controllers;
using ChatCommunicator.Contracts.Dtos.Friendships;

namespace ChatCommunicator.Application.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class FriendsController : BaseController
    {
        private readonly IFriendsService _friendsManager;
        private readonly IChatService _chatService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<FriendsController> _logger;

        public FriendsController(IFriendsService friendsManager,
            IChatService chatService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<FriendsController> logger)
        {
            _friendsManager = friendsManager;
            _chatService = chatService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Send Invite
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to send a friend invitation by providing the sender's and recipient's GUIDs.
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
        /// POST /api/user/send-invite
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
        [HttpPost("send-invite")]
        [ProducesResponseType(typeof(SendInviteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendInviteAsync(InviteDto inviteDto)
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
        /// Get Invitations
        /// </summary>
        /// <remarks>
        /// Returns all pending friend invitations for the specified user GUID.
        /// If the user does not exist or input is invalid, returns appropriate error responses.
        /// </remarks>
        /// <param name="userId">GUID of the user whose invitations to retrieve.</param>
        /// <returns>
        /// List of users who sent invitations, or an error response.
        /// </returns>
        /// <response code="200">Invitations retrieved successfully.</response>
        /// <response code="400">Invalid user ID (e.g., malformed GUID).</response>
        /// <response code="401">Unauthorized - JWT token required.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">Unexpected server error.</response>
        /// <example>
        /// <code>
        /// GET /api/user/get-invitations/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// </code>
        /// </example>
        [Authorize]
        [HttpGet("get-invitations/{userId}")]
        [ProducesResponseType(typeof(List<SimpleUserWithAvatarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvitationsAsync(Guid userId)
        {
            var result = await _friendsManager.GetInvitationsAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[GetInvitationsAsync] Successfully retrieved invitations for UserId: {UserId}. Count: {Count}",
                    userId, result.Value?.Count() ?? 0);
                return Ok(result.Value);
            }

            return HandleError(result, "GetInvitationsAsync", _logger);
        }



        /// <summary>
        /// Get Users to Invite by Text
        /// </summary>
        /// <remarks>
        /// Authenticated users can search for other users by username or display name.
        /// The search excludes users who are already friends or already invited.
        /// Requires a valid JWT token with user GUID.
        /// </remarks>
        /// <param name="text">Text to search users by.</param>
        /// <returns>
        /// List of users matching the search criteria available for invitation, or an error response.
        /// </returns>
        /// <response code="200">Users retrieved successfully.</response>
        /// <response code="400">Invalid request (e.g., empty or malformed input).</response>
        /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
        /// <response code="404">No matching users found.</response>
        /// <response code="500">Unexpected server error.</response>
        /// <example>
        /// <code>
        /// GET /api/user/get-users-to-invite-by-text/john
        /// </code>
        /// </example>
        [Authorize]
        [HttpGet("get-users-to-invite-by-text/{text}")]
        [ProducesResponseType(typeof(List<UserToInviteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersToInviteByTextAsync(string text)
        {
            var validate = ValidateAndGetUserId("GetUsersToInviteByTextAsync", _logger, out Guid userId);

            if (validate != null)
            {
                return validate;
            }

            var result = await _friendsManager.GetUsersToInviteByTextAsync(userId, text);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[GetUsersToInviteByTextAsync] Successfully retrieved users to invite for UserId: {UserId}, SearchText: {Text}. Count: {Count}",
                    userId, text, result.Value?.Count() ?? 0);
                return Ok(result.Value);
            }

            return HandleError(result, "GetUsersToInviteByTextAsync", _logger);
        }


        /// <summary>
        /// Deceline Invite
        /// </summary>
        /// <remarks>
        /// Allows an authenticated user to decline a friend invitation by providing sender and recipient GUIDs.
        /// Returns appropriate errors if the invitation is invalid, already handled, or missing.
        /// </remarks>
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
        /// POST /api/user/deceline-invite
        /// {
        ///     "senderId": "b0f4e7d2-115a-4dcf-b9f5-5b08f6bcb034",
        ///     "recipientId": "a54ff5c4-6f67-4c10-b4a7-b6d3f0d8c9cb"
        /// }
        /// </code>
        /// </example>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Authorize]
        [HttpPost("deceline-invite")]
        public async Task<IActionResult> DecelineInviteAsync(InviteDto inviteDto)
        {
            var result = await _friendsManager.DecelineInviteAsync(inviteDto.SenderId, inviteDto.RecipientId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[DecelineInviteAsync] Invite declined successfully. SenderId: {SenderId}, RecipientId: {RecipientId}",
                    inviteDto.SenderId, inviteDto.RecipientId);
                return Ok();
            }

            return HandleError(result, "DecelineInviteAsync", _logger);
        }


        /// <summary>
        /// Accept Invite
        /// </summary>
        /// <remarks>
        /// Allows an authenticated user to accept a friend invitation by sender and recipient GUIDs.
        /// On success, users are added to each other's friend lists.
        /// Returns errors for invalid input, not found invitations, or conflicts.
        /// </remarks>
        /// <param name="acceptInviteDto">Invitation data with sender and recipient GUIDs.</param>
        /// <returns>
        /// HTTP 200 on success or an error describing the issue.
        /// </returns>
        /// <response code="200">Invitation accepted; users are now friends.</response>
        /// <response code="400">Invalid data provided.</response>
        /// <response code="401">Unauthorized - JWT token required.</response>
        /// <response code="404">Invitation not found.</response>
        /// <response code="409">Conflict - users already friends or invitation accepted.</response>
        /// <response code="500">Unexpected server error.</response>
        /// <example>
        /// <code>
        /// POST /api/user/acceptInvite
        /// {
        ///     "senderId": "b0f4e7d2-115a-4dcf-b9f5-5b08f6bcb034",
        ///     "recipientId": "a54ff5c4-6f67-4c10-b4a7-b6d3f0d8c9cb"
        /// }
        /// </code>
        /// </example>
        [Authorize]
        [HttpPost("accept-invite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AcceptInviteAsync(InviteDto acceptInviteDto)
        {
            var addFriendsResult = await _friendsManager.AddFriendsAsync(acceptInviteDto.SenderId, acceptInviteDto.RecipientId);
            var createConversationResult = await _chatService.GetOrCreateConversationAsync(acceptInviteDto.SenderId, acceptInviteDto.RecipientId);

            if (addFriendsResult.IsSuccess && createConversationResult.IsSuccess)
            {
                _logger.LogInformation("[AcceptInviteAsync] Friends added and conversation created successfully. SenderId: {SenderId}, RecipientId: {RecipientId}",
                    acceptInviteDto.SenderId, acceptInviteDto.RecipientId);
                return Ok();
            }

            if (!addFriendsResult.IsSuccess)
                return HandleError(addFriendsResult, "AcceptInviteAsync", _logger);

            return HandleError(createConversationResult, "AcceptInviteAsync", _logger);
        }


        /// <summary>
        /// Get Friends
        /// </summary>
        /// <remarks>
        /// Fetches all users marked as friends of the specified user GUID.
        /// Returns errors for invalid input or unexpected issues.
        /// </remarks>
        /// <param name="userId">GUID of the user whose friends to retrieve.</param>
        /// <returns>
        /// List of friends or an error response.
        /// </returns>
        /// <response code="200">Friends list retrieved successfully.</response>
        /// <response code="400">Invalid user ID.</response>
        /// <response code="401">Unauthorized - JWT token required.</response>
        /// <response code="500">Unexpected server error.</response>
        /// <example>
        /// <code>
        /// GET /api/user/get-friends/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// </code>
        /// </example>
        [Authorize]
        [HttpGet("get-friends/{userId}")]
        [ProducesResponseType(typeof(List<SimpleUserWithAvatarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFriendsAsync(Guid userId)
        {
            var result = await _friendsManager.GetFriendsAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[GetFriendsAsync] Friends list fetched successfully. UserId: {UserId}", userId);
                return Ok(result.Value);
            }

            return HandleError(result, "GetFriendsAsync", _logger);
        }

    }
}