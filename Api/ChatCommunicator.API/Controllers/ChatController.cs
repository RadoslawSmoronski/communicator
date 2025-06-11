using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using System.Security.Claims;
using ChatCommunicator.Contracts.Dtos.Chat;
using ChatCommunicator.Shared.Result;
using ChatCommunicator.Application.Services.Interfaces;

namespace ChatCommunicator.Application.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IChatService _chatService;

        public ChatController(
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IChatService chatManager)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _chatService = chatManager;
        }

        /// <summary>
        /// Retrieves all chat conversations for the authenticated user.
        /// </summary>
        /// <remarks>
        /// This endpoint returns all chat conversations associated with the currently authenticated user.
        /// A valid JWT token must be included in the Authorization header. The user's ID is extracted
        /// from the token claims and used to fetch the relevant chat data.
        ///
        /// If the user ID is missing or not a valid GUID, a 401 Unauthorized response is returned.
        /// Appropriate error responses are returned in case of validation failures or internal errors.
        /// </remarks>
        /// <returns>
        /// A list of chat conversations belonging to the authenticated user, or an error response.
        /// </returns>
        /// <response code="200">Successfully retrieved the list of chats.</response>
        /// <response code="400">Invalid input or validation error.</response>
        /// <response code="401">Unauthorized – missing or invalid JWT token.</response>
        /// <response code="500">Internal server error.</response>
        /// <example>
        /// GET /api/chat/get-chats
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
        /// </example>
        [Authorize]
        [HttpGet("get-chats")]
        [ProducesResponseType(typeof(List<ChatDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetChatsAsync()
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

            var result = await _chatService.GetChatsAsync(userId);

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

        /// <summary>
        /// Retrieves paginated messages from a specific conversation.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a paginated list of messages from a specific conversation,
        /// starting after a given message ID. It requires a valid JWT token in the Authorization header.
        ///
        /// Only messages from conversations the user has access to will be returned.
        /// If the conversation ID or message ID is invalid, or the user is unauthorized to access the conversation,
        /// appropriate error responses are returned.
        /// </remarks>
        /// <param name="getMessagesDto">
        /// DTO containing the conversation ID and the starting message ID for pagination.
        /// </param>
        /// <returns>
        /// A paginated list of messages, or an appropriate error response.
        /// </returns>
        /// <response code="200">Successfully retrieved the list of messages.</response>
        /// <response code="400">Invalid input or validation error (e.g., malformed GUID).</response>
        /// <response code="401">Unauthorized – missing or invalid JWT token.</response>
        /// <response code="404">Conversation not found or access denied.</response>
        /// <response code="500">Internal server error.</response>
        /// <example>
        /// POST /api/chat/get-paged-messages
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
        /// {
        ///   "conversationId": "123e4567-e89b-12d3-a456-426614174000",
        ///   "fromMessageId": "789e4567-e89b-12d3-a456-426614174999"
        /// }
        /// </example>
        [Authorize]
        [HttpGet("get-paged-messages")]
        [ProducesResponseType(typeof(List<MessageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagedMessagesAsync([FromBody] GetPagedMessagesDto getMessagesDto)
        {
            var result = await _chatService.GetPagedMessagesFromMessageIdAsync(getMessagesDto.ConversationId, getMessagesDto.FromMessageId);

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
    }
}
