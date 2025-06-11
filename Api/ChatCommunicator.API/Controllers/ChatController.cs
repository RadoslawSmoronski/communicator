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
        /// GetChats.
        /// </summary>
        /// <remarks>
        /// This endpoint returns all chat conversations that belong to the currently authenticated user.
        /// A valid JWT token must be provided in the Authorization header. The user's ID is extracted from
        /// the token claims and used to fetch associated chat data.
        /// 
        /// If the user's ID is missing or not a valid GUID, a 401 Unauthorized response is returned.
        /// If a validation or internal error occurs, appropriate error responses are provided.
        /// </remarks>
        /// <returns>
        /// A list of chat conversations associated with the authenticated user, or an error response.
        /// </returns>
        /// <response code="200">Returns the list of chats.</response>
        /// <response code="400">Invalid input or validation failure.</response>
        /// <response code="401">Unauthorized – missing or invalid JWT token.</response>
        /// <response code="500">Unexpected server error occurred.</response>
        /// <example>
        /// GET /api/user/getChats
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
        /// </example>
        [Authorize]
        [HttpGet("getChats")]
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
        /// GetPagedMessages.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a subset of messages from a given conversation, starting after the specified message ID.
        /// It requires a valid JWT token provided in the Authorization header. The user's identity is inferred from the token,
        /// and only messages from conversations that the user has access to will be returned.
        /// 
        /// If the conversation ID or message ID is invalid, or the user does not have access, appropriate error responses will be returned.
        /// </remarks>
        /// <param name="getMessagesDto">DTO containing the conversation ID and the starting message ID for pagination.</param>
        /// <returns>
        /// A paginated list of messages or an error response.
        /// </returns>
        /// <response code="200">Returns the paginated list of messages.</response>
        /// <response code="400">Invalid input or validation failure (e.g. malformed GUID).</response>
        /// <response code="401">Unauthorized – missing or invalid JWT token.</response>
        /// <response code="404">Conversation not found or user does not have access.</response>
        /// <response code="500">Unexpected server error occurred.</response>
        /// <example>
        /// POST /api/user/getPagedMessages
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
        /// {
        ///   "conversationId": "123e4567-e89b-12d3-a456-426614174000",
        ///   "fromMessageId": "789e4567-e89b-12d3-a456-426614174999"
        /// }
        /// </example>
        [Authorize]
        [HttpGet("getPagedMessages")]
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
