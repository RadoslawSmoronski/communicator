using Api.Hubs;
using Api.Hubs.Interfaces;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Chat;
using Api.Models.Dtos.Chat;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SignalRJWTServer.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IUsersConnectionManager _usersConnectionManager;
        private readonly IChatManager _chatManager;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;

        public ChatHub(IUsersConnectionManager usersConnectionManager,
            IChatManager chatManager,
            UserManager<UserAccount> userManager,
            IMapper mapper)
        {
            _usersConnectionManager = usersConnectionManager;
            _chatManager = chatManager;
            _userManager = userManager;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User!.Identity.Name;
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _usersConnectionManager.AddUpdateAsync(Context.ConnectionId, userId);

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string recipientId, string conversationId, string content) // Needs tests
        {
            var userName = Context.User!.Identity.Name;
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var recipientConnectionsId = _usersConnectionManager.GetUserConnectionsId(recipientId);

            var conversation = await _chatManager.GetOrCreateConversationAsync(userId, recipientId);
            var sender = await _userManager.FindByIdAsync(userId);

            if (sender == null)
            {
                return;
            }

            var message = new Message()
            {
                ConversationId = Guid.Parse(conversationId),
                Conversation = conversation.Value,
                SenderId = userId,
                Sender = sender,
                Content = content,
            };

            if (recipientConnectionsId != null)
            {
                var messageDto = _mapper.Map<MessageDto>(message);

                await Clients.Clients(recipientConnectionsId).ReceiveMessage(messageDto);
            }

            await _chatManager.SaveMessageAsync(message);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _usersConnectionManager.RemoveAsync(Context.ConnectionId, userId);

            await base.OnDisconnectedAsync(exception);
        }

    }
}
