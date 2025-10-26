using API.Controllers;
using API.DTOs;
using Application.Chats.Queries.GetPagedMessages;
using Application.Common.Interfaces;
using Application.Friendships.Commands.DeleteFriendship;
using ChatCommunicator.Application.Hubs;
using ChatCommunicator.Application.Hubs.Interfaces;
using Domain.Entities;
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
        private readonly IUser _user;

        public ChatsController(
            IHubContext<ChatHub, IChatClient> chatHubContext,
            ILogger<ChatsController> logger,
            ISender sender,
            IUser user)
        {
            _chatHubContext = chatHubContext;
            _logger = logger;
            _sender = sender;
            _user = user;
        }

        [Authorize] // refactor: docs
        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetPagedMessagesAsync([FromRoute] Guid conversationId, [FromQuery] Guid? fromMessageId)
        {
            if(_user.Id is null)
            {
                return NotFound();
            }

            var userId = _user.Id.Value;

            if (fromMessageId == null && fromMessageId == Guid.Empty) //REFACTOR , TEMP
            {
                return Ok();
            }

            var query = new GetPagedMessagesQuery(conversationId, userId, fromMessageId.Value);
            var result = await _sender.Send(query);

            if (result.IsSuccess && result.Value != null && result.Value.PagedMessagesDto != null)
            {

                if (result.Value.RecipientConnectionsId != null &&
                   result.Value.UserReadMessageId != null)
                {

                    await _chatHubContext.Clients.Clients(result.Value.RecipientConnectionsId)
                        .MessageRead(new MessageReadDto { MessageId = result.Value.UserReadMessageId.Value, ConversationId = conversationId });
                }

                return Ok(result.Value.PagedMessagesDto);
            }

            return HandleError(result, "GetPagedMessagesAsync", _logger);
        }

    }
}
