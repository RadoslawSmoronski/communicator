using AutoMapper;
using ChatCommunicator.API.Controllers;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts.Dtos.Chat;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatCommunicator.Application.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;   

        public ChatController(
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IChatService chatManager,
            ILogger<ChatController> logger)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _chatService = chatManager;
            _logger = logger;
        }

        /// <summary>
        /// Get Chats
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
            var userId = GetUserIdByClaims();

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("[GetChatsAsync] Unauthorized access attempt: JWT Token is missing.");
                return Problem(
                    statusCode: 401,
                    title: "Unauthorized",
                    detail: "JWT Token is not valid."
                );
            }

            var result = await _chatService.GetChatsAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[GetChatsAsync] Successfully retrieved chats for UserId: {UserId}. Count: {Count}", userId, result.Value?.Count() ?? 0);
                return Ok(result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Validation)
                {
                    _logger.LogWarning("[GetChatsAsync] Validation error for UserId: {UserId}. Message: {ErrorMessage}", userId, errorMessage);
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: errorMessage
                    );
                }

                _logger.LogError("[GetChatsAsync] Unexpected error for UserId: {UserId}. ErrorType: {ErrorType}. Message: {ErrorMessage}", userId, errorCode, errorMessage);
                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: "An unexpected error occurred."
                );
            }

            _logger.LogError("[GetChatsAsync] Unexpected null error for UserId: {UserId}", userId);
            return Problem(
                statusCode: 500,
                title: "InternalServerError",
                detail: "An unexpected error occurred."
            );
        }


        /// <summary>
        /// Get Paged Messages
        /// </summary>
        /// <remarks>
        /// This endpoint returns a paginated list of messages from a conversation.  
        /// If <c>fromMessageId</c> is provided, it returns messages after that ID;  
        /// if <c>fromMessageId</c> is null, it returns an empty list (with 200 OK).  
        /// A valid JWT token must be included in the Authorization header.  
        /// If <c>conversationId</c> is missing, empty, or invalid, an error response is returned.
        /// </remarks>
        /// <param name="conversationId">The ID of the conversation (GUID). Required.</param>
        /// <param name="fromMessageId">The ID of the message after which to start fetching (GUID). Optional.</param>
        /// <returns>A list of messages (possibly empty) or an error response.</returns>
        /// <response code="200">Successfully retrieved the list of messages (can be empty if fromMessageId is null).</response>
        /// <response code="400">Invalid input or validation error.</response>
        /// <response code="401">Unauthorized – missing or invalid JWT token.</response>
        /// <response code="404">Conversation not found or access denied.</response>
        /// <response code="500">Internal server error.</response>
        /// <example>
        /// GET /api/chat/get-paged-messages?conversationId=123e4567-e89b-12d3-a456-426614174000
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
        /// </example>
        [Authorize]
        [HttpGet("get-paged-messages")]
        [ProducesResponseType(typeof(List<MessageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagedMessagesAsync(Guid? conversationId, Guid? fromMessageId)
        {
            var result = await _chatService.GetPagedMessagesFromMessageIdAsync(conversationId, fromMessageId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[GetPagedMessagesAsync] Successfully retrieved paged messages. ConversationId: {ConversationId}, FromMessageId: {FromMessageId}, Count: {Count}",
                    conversationId, fromMessageId, result.Value?.Count() ?? 0);
                return Ok(result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Validation)
                {
                    _logger.LogWarning("[GetPagedMessagesAsync] Validation error. ConversationId: {ConversationId}, FromMessageId: {FromMessageId}. Message: {ErrorMessage}",
                        conversationId, fromMessageId, errorMessage);
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: errorMessage
                    );
                }
                else if (errorCode == ErrorType.NotFound)
                {
                    _logger.LogWarning("[GetPagedMessagesAsync] Not found error. ConversationId: {ConversationId}, FromMessageId: {FromMessageId}. Message: {ErrorMessage}",
                        conversationId, fromMessageId, errorMessage);
                    return Problem(
                        statusCode: 404,
                        title: "Not Found",
                        detail: errorMessage
                    );
                }

                _logger.LogError("[GetPagedMessagesAsync] Unexpected error. ConversationId: {ConversationId}, FromMessageId: {FromMessageId}. ErrorType: {ErrorType}. Message: {ErrorMessage}",
                    conversationId, fromMessageId, errorCode, errorMessage);
                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: "An unexpected error occurred."
                );
            }

            _logger.LogError("[GetPagedMessagesAsync] Unexpected null error. ConversationId: {ConversationId}, FromMessageId: {FromMessageId}",
                conversationId, fromMessageId);
            return Problem(
                statusCode: 500,
                title: "InternalServerError",
                detail: "An unexpected error occurred."
            );
        }

    }
}
