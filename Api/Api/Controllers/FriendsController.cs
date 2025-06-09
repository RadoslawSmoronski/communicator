using ChatCommunicator.Models.Dtos.Controllers.FriendsController;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChatCommunicator.Managers.Interfaces;
using AutoMapper;
using ChatCommunicator.Models.Dtos;
using System.Security.Claims;
using ChatCommunicator.Shared.Result;

namespace ChatCommunicator.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class FriendsController : Controller
    {
        private readonly IFriendsManager _friendsManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FriendsController(IFriendsManager friendsManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _friendsManager = friendsManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// SendInviteAsync.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to send a friend invitation to another user
        /// by providing sender and recipient GUIDs. The system returns an appropriate response based on
        /// the outcome, including validation errors, conflicts, or if the recipient was not found.
        /// </remarks>
        /// <param name="inviteDto">The invitation data including sender and recipient user GUIDs.</param>
        /// <returns>
        /// A response indicating success or a specific error describing why the invitation could not be processed.
        /// </returns>
        /// <response code="200">Invitation sent successfully.</response>
        /// <response code="400">Invalid invitation data (e.g., malformed or missing GUIDs).</response>
        /// <response code="401">Unauthorized – valid JWT token is required.</response>
        /// <response code="404">The recipient user was not found.</response>
        /// <response code="409">A conflict occurred (e.g., invitation already exists).</response>
        /// <response code="500">Unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// POST /api/user/sendInviteAsync
        /// {
        ///     "senderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///     "recipientId": "d2
        ///     719f9d-8f8c-4b5a-80c4-07afcf1c5b90"
        /// }
        /// </code>
        /// </example>
        [Authorize]
        [HttpPost("sendInviteAsync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
                Ok();
            }

            if (inviteResult.Error != null)
            {
                var errorCode = inviteResult.Error.ErrorType;
                var errorMessage = inviteResult.Error.Description;

                if (errorCode == ErrorType.Validation)
                {
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: errorMessage
                    );
                }
                else if (errorCode == ErrorType.NotFound)
                {
                    return Problem(
                        statusCode: 404,
                        title: "Not Found",
                        detail: errorMessage
                    );
                }
                else if (errorCode == ErrorType.Conflict)
                {
                    return Problem(
                        statusCode: 409,
                        title: "Conflict",
                        detail: errorMessage
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: "An unexpected error occurred."
                );
            }

            return Problem(
                statusCode: 500,
                title: "InternalServerError",
                detail: "An unexpected error occurred."
            );
        }

        /// <summary>
        /// GetInvitations.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to retrieve all friend invitations
        /// addressed to the specified user by their GUID. If the user does not exist,
        /// a 404 response is returned. In case of invalid input or other issues, an appropriate error is returned.
        /// </remarks>
        /// <param name="userId">The GUID of the user whose invitations should be retrieved.</param>
        /// <returns>
        /// A response containing a list of users who have sent invitations to the specified user,
        /// or an error describing the problem.
        /// </returns>
        /// <response code="200">Invitations successfully retrieved.</response>
        /// <response code="400">Invalid user ID (e.g., malformed GUID).</response>
        /// <response code="401">Unauthorized – valid JWT token is required.</response>
        /// <response code="404">The specified user was not found.</response>
        /// <response code="500">Unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// GET /api/user/getInvitations/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// </code>
        /// </example>
        [Authorize]
        [HttpGet("getInvitations/{userId}")]
        [ProducesResponseType(typeof(List<SimpleUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInvitationsAsync(Guid userId)
        {
            var result = await _friendsManager.GetInvitationsAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Validation)
                {
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: errorMessage
                    );
                }
                else if (errorCode == ErrorType.NotFound)
                {
                    return Problem(
                        statusCode: 404,
                        title: "Not Found",
                        detail: errorMessage
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: "An unexpected error occurred."
                );
            }

            return Problem(
                statusCode: 500,
                title: "InternalServerError",
                detail: "An unexpected error occurred."
            );
        }


        /// <summary>
        /// GetUsersToInviteByText.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to search for other users to invite by providing a text query.
        /// The search is performed server-side, excluding users that are already invited or already friends.
        /// The user making the request must be authenticated via a valid JWT token containing a valid GUID identifier.
        /// </remarks>
        /// <param name="text">The text to match usernames or display names against.</param>
        /// <returns>
        /// A response containing a list of users available for invitation matching the search criteria,
        /// or an error describing the problem.
        /// </returns>
        /// <response code="200">List of users retrieved successfully.</response>
        /// <response code="400">Invalid request (e.g., empty or malformed input).</response>
        /// <response code="401">Unauthorized – JWT token missing, invalid, or malformed GUID.</response>
        /// <response code="404">No matching users found.</response>
        /// <response code="500">Unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// GET /api/user/getUsersToInviteByText/john
        /// </code>
        /// </example>
        [Authorize]
        [HttpGet("getUsersToInviteByText/{text}")]
        [ProducesResponseType(typeof(List<UserToInviteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersToInviteByTextAsync(string text)
        {
            var userClaimId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userClaimId == null)
            {
                return Problem(
                    statusCode: 401,
                    title: "Unauthorized",
                    detail: "JWT Token is not valid."
                );
            }


            if (!Guid.TryParse(userClaimId, out Guid userId))
            {
                return Problem(
                    statusCode: 401,
                    title: "Unauthorized",
                    detail: "UserId from JWT Token is not a guid."
                );
            }

            var result = await _friendsManager.GetUsersToInviteByTextAsync(userId, text);

            if (result.IsSuccess)
            {
                Ok(result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Validation)
                {
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: errorMessage
                    );
                }
                else if (errorCode == ErrorType.NotFound)
                {
                    return Problem(
                        statusCode: 404,
                        title: "Not Found",
                        detail: errorMessage
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: "An unexpected error occurred."
                );
            }

            return Problem(
                statusCode: 500,
                title: "InternalServerError",
                detail: "An unexpected error occurred."
            );
        }

        /// <summary>
        /// DecelineInvite.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to decline a friend invitation from another user.
        /// Both sender and recipient GUIDs must be provided in the request body. If the invitation does not exist
        /// or has already been handled, a 404 or 400 response is returned accordingly.
        /// </remarks>
        /// <param name="inviteDto">The invitation data containing sender and recipient user GUIDs.</param>
        /// <returns>
        /// A response indicating whether the invitation was successfully declined or an error describing the issue.
        /// </returns>
        /// <response code="200">Invitation declined successfully.</response>
        /// <response code="400">Invalid data provided (e.g., malformed or missing GUIDs).</response>
        /// <response code="401">Unauthorized – valid JWT token is required.</response>
        /// <response code="404">Invitation not found or already declined.</response>
        /// <response code="500">Unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// POST /api/user/decelineInvite
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
        [HttpPost("decelineInvite")]
        public async Task<IActionResult> DecelineInviteAsync(InviteDto inviteDto)
        {
            var result = await _friendsManager.DecelineInviteAsync(inviteDto.SenderId, inviteDto.RecipientId);

            if (result.IsSuccess)
            {
                return Ok();
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Validation)
                {
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: errorMessage
                    );
                }
                else if (errorCode == ErrorType.NotFound)
                {
                    return Problem(
                        statusCode: 404,
                        title: "Not Found",
                        detail: errorMessage
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: "An unexpected error occurred."
                );
            }

            return Problem(
                statusCode: 500,
                title: "InternalServerError",
                detail: "An unexpected error occurred."
            );
        }

        /// <summary>
        /// AcceptInvite.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to accept a friend invitation by providing the sender and recipient GUIDs.
        /// On success, the users are added to each other's friend lists. If the invitation does not exist,
        /// is invalid, or a conflict occurs (e.g., users are already friends), an appropriate error is returned.
        /// </remarks>
        /// <param name="acceptInviteDto">The invitation data containing sender and recipient user GUIDs.</param>
        /// <returns>
        /// A response indicating whether the invitation was successfully accepted or an error describing the issue.
        /// </returns>
        /// <response code="200">Invitation accepted successfully; users are now friends.</response>
        /// <response code="400">Invalid data provided (e.g., malformed or missing GUIDs).</response>
        /// <response code="401">Unauthorized – valid JWT token is required.</response>
        /// <response code="404">Invitation not found.</response>
        /// <response code="409">Conflict – users are already friends or invitation was already accepted.</response>
        /// <response code="500">Unexpected server error occurred.</response>
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
        [HttpPost("acceptInvite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AcceptInviteAsync(InviteDto acceptInviteDto)
        {
            var result = await _friendsManager.AddFriendsAsync(acceptInviteDto.SenderId, acceptInviteDto.RecipientId);

            if (result.IsSuccess)
            {
                Ok();
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Validation)
                {
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: errorMessage
                    );
                }
                else if (errorCode == ErrorType.NotFound)
                {
                    return Problem(
                        statusCode: 404,
                        title: "Not Found",
                        detail: errorMessage
                    );
                }
                else if (errorCode == ErrorType.Conflict)
                {
                    return Problem(
                        statusCode: 409,
                        title: "Conflict",
                        detail: errorMessage
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: "An unexpected error occurred."
                );
            }

            return Problem(
                statusCode: 500,
                title: "InternalServerError",
                detail: "An unexpected error occurred."
            );
        }

        /// <summary>
        /// GetFriends.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to fetch all users that are marked as friends
        /// of the specified user identified by their GUID. If the input is invalid, or an unexpected error occurs,
        /// the appropriate status code and message are returned.
        /// </remarks>
        /// <param name="userId">The GUID of the user whose friends should be retrieved.</param>
        /// <returns>
        /// A response containing a list of friends for the specified user,
        /// or an error response if the request could not be completed.
        /// </returns>
        /// <response code="200">List of friends retrieved successfully.</response>
        /// <response code="400">Invalid user ID (e.g., malformed GUID).</response>
        /// <response code="401">Unauthorized – valid JWT token is required.</response>
        /// <response code="500">Unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// GET /api/user/getFriends/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// </code>
        /// </example>
        [Authorize]
        [HttpGet("getFriends/{userId}")]
        [ProducesResponseType(typeof(List<SimpleUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFriendsAsync(Guid userId)
        {
            var result = await _friendsManager.GetFriendsAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Validation)
                {
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: errorMessage
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: "An unexpected error occurred."
                );
            }

            return Problem(
                statusCode: 500,
                title: "InternalServerError",
                detail: "An unexpected error occurred."
            );
        }
    }
}