using API.Contracts.Chats;
using API.Controllers;
using Application.Chats.Queries.GetPagedMessages;
using Application.Common.Interfaces;
using Application.Contracts.Chat;
using ChatCommunicator.Application.Hubs;
using ChatCommunicator.Application.Hubs.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatCommunicator.Application.Controllers
{
    [Route("api/chats")]
    [ApiController]
    public class ChatsController : BaseController
    {
        private readonly IHubContext<ChatHub, IChatClient> _chatHubContext;
        private readonly ILogger<ChatsController> _logger;
        private readonly ISender _sender;
        private readonly ICurrentUser _user;

        public ChatsController(
            IHubContext<ChatHub, IChatClient> chatHubContext,
            ILogger<ChatsController> logger,
            ISender sender,
            ICurrentUser user)
        {
            _chatHubContext = chatHubContext;
            _logger = logger;
            _sender = sender;
            _user = user;
        }

        /// <summary>
        /// Get paged messages
        /// </summary>
        /// <param name="conversationId">The identifier of the conversation to fetch messages from.</param>
        /// <param name="fromMessageId">
        /// Optional anchor message identifier. When provided, the page of messages is returned relative to this message
        /// (typically older messages). When omitted, the latest page may be returned depending on implementation.
        /// </param>
        /// <returns>
        /// Returns 200 OK with a <see cref="GetPagedMessagesResponse"/> containing the messages and the last friend-read message id;
        /// otherwise an error response.
        /// </returns>
        /// <remarks>
        /// Route: GET api/chats/{conversationId}/messages
        /// Authorization: Required.
        /// Side effects: May emit a "message read" notification to recipients via SignalR when applicable.
        /// </remarks>
        /// <response code="200">The requested page of messages is returned.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">The user is not authorized to access the conversation.</response>
        /// <response code="404">The user or conversation was not found.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        [Authorize]
        [HttpGet("{conversationId}/messages")]
        [ProducesResponseType(typeof(GetPagedMessagesResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> GetPagedMessagesAsync([FromRoute] Guid conversationId, [FromQuery] Guid? fromMessageId)
        {
            if (_user.Id is null)
            {
                return Unauthorized();
            }

            var userId = _user.Id.Value;

            var query = new GetPagedMessagesQuery(conversationId, userId, fromMessageId);
            var result = await _sender.Send(query);

            if (result.IsSuccess && result.Value != null && result.Value.Messages != null)
            {
                if (result.Value.RecipientConnectionsId != null &&
                    result.Value.UserReadMessageId != null)
                {
                    await _chatHubContext.Clients.Clients(result.Value.RecipientConnectionsId)
                        .MessageRead(new MessageReadEvent(MessageId: result.Value.UserReadMessageId.Value, ConversationId: conversationId));

                }

                return Ok(new GetPagedMessagesResponse(
                    Messages: result.Value.Messages,
                    LastFriendReadMessageId: result.Value.LastFriendReadMessageId
                ));
            }

            return HandleError(result, "ChatsController - GetPagedMessagesAsync", _logger);
        }

    }
}
