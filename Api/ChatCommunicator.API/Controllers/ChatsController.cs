using ChatCommunicator.API.Controllers;
using ChatCommunicator.Application.Hubs;
using ChatCommunicator.Application.Hubs.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts.Dtos.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatCommunicator.Application.Controllers
{
    [Route("api/chats")]
    [ApiController]
    public class ChatsController : BaseController
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub, IChatClient> _chatHubContext;
        private readonly ILogger<ChatsController> _logger;   

        public ChatsController(
            IChatService chatManager,
            IHubContext<ChatHub, IChatClient> chatHubContext,
            ILogger<ChatsController> logger)
        {
            _chatService = chatManager;
            _chatHubContext = chatHubContext;
            _logger = logger;
        }

        /// <summary>
        /// Get chats
        /// </summary>
        /// <remarks>
        /// Requires a valid JWT token in the Authorization header. The user's ID is extracted
        /// from the token claims and used to fetch chat data.
        ///
        /// Returns 401 Unauthorized if the user ID is missing or invalid.
        /// Returns appropriate error responses for validation failures or internal errors.
        /// </remarks>
        /// <returns>
        /// A list of chat conversations for the authenticated user, or an error response.
        /// </returns>
        /// <response code="200">List of chats successfully retrieved.</response>
        /// <response code="400">Invalid input or validation error.</response>
        /// <response code="401">Unauthorized – missing or invalid JWT token.</response>
        /// <response code="500">Internal server error.</response>
        /// <example>
        /// GET /api/chats
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
        /// </example>
        [Authorize]
        [HttpGet()]
        [ProducesResponseType(typeof(List<ChatDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChatsAsync()
        {
            var validate = ValidateAndGetUserId("GetChatsAsync", _logger, out Guid userId);

            if (validate != null)
            {
                return validate;
            }

            var result = await _chatService.GetChatsAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[GetChatsAsync] Successfully retrieved chats for UserId: {UserId}. Count: {Count}", userId, result.Value?.Count() ?? 0);
                return Ok(result.Value);
            }

            return HandleError(result, "GetChatsAsync", _logger);
        }


        /// <summary>
        /// Get a paginated list of messages
        /// </summary>
        /// <remarks>
        /// This endpoint returns a paginated list of messages for a given conversation.
        /// If <c>fromMessageId</c> is provided, it returns messages after that ID.
        /// If <c>fromMessageId</c> is null, it returns an empty list (with 200 OK).
        /// Requires a valid JWT token in the Authorization header.
        /// If <c>conversationId</c> is missing, empty, or invalid, an error response is returned.
        /// <br/><br/>
        /// If messages are successfully retrieved and both the recipient is online
        /// (has active connections) and there's a last read message ID from the friend,
        /// a SignalR notification is sent to the recipient with the MessageRead event containing
        /// the last read message ID.
        /// </remarks>
        /// <param name="conversationId">The ID of the conversation (GUID). Required.</param>
        /// <param name="fromMessageId">The ID of the message after which to start fetching (GUID). Optional.</param>
        /// <returns>
        /// A paged result of messages (<see cref="PagedMessagesDto"/>) or an error response.
        /// </returns>
        /// <response code="200">Successfully retrieved the paged messages (can be empty if <c>fromMessageId</c> is null).</response>
        /// <response code="400">Invalid input or validation error.</response>
        /// <response code="401">Unauthorized – missing or invalid JWT token.</response>
        /// <response code="404">Conversation not found or access denied.</response>
        /// <response code="500">Internal server error.</response>
        /// <example>
        /// GET /api/chats/{conversationId}/messages?fromMessageId=123e4567-e89b-12d3-a456-426614174000
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
        /// </example>
        [Authorize]
        [HttpGet("{conversationId}/messages")]
        [ProducesResponseType(typeof(PagedMessagesDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPagedMessagesAsync(Guid conversationId, Guid? fromMessageId)
        {
            var validate = ValidateAndGetUserId("GetPagedMessagesAsync", _logger, out Guid userId);

            if (validate != null)
            {
                return validate;
            }

            var result = await _chatService.GetPagedMessagesFromMessageIdAsync(conversationId, userId, fromMessageId);

            if (result.IsSuccess && result.Value != null && result.Value.PagedMessagesDto != null)
            {
                _logger.LogInformation("[GetPagedMessagesAsync] Successfully retrieved paged messages. ConversationId: {ConversationId}, FromMessageId: {FromMessageId}",
                    conversationId, fromMessageId);


                if (result.Value.RecipientConnectionsId != null &&
                   result.Value.PagedMessagesDto.LastFriendReadMessageId != null)
                {
                    _logger.LogInformation("[GetPagedMessagesAsync] Sending MessageRead notification to recipient. RecipientConnectionsCount: {ConnectionCount}, LastReadMessageId: {LastReadMessageId}",
                        result.Value.RecipientConnectionsId.Count,
                        result.Value.PagedMessagesDto.LastFriendReadMessageId.Value);

                    await _chatHubContext.Clients.Clients(result.Value.RecipientConnectionsId)
                        .MessageRead(result.Value.PagedMessagesDto.LastFriendReadMessageId.Value);
                }

                return Ok(result.Value.PagedMessagesDto);
            }

            return HandleError(result, "GetPagedMessagesAsync", _logger);
        }

    }
}
