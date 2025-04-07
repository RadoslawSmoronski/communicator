using Api.Managers;
using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;

namespace SignalRJWTServer.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUsersConnectionManager _usersConnectionManager;
        private readonly IChatManager _chatManager;
        private readonly UserManager<UserAccount> _userManager;
        
        public ChatHub(IUsersConnectionManager usersConnectionManager, IChatManager chatManager, UserManager<UserAccount> userManager)
        {
            _usersConnectionManager = usersConnectionManager;
            _chatManager = chatManager;
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User!.Identity.Name;
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _usersConnectionManager.AddUpdateAsync(Context.ConnectionId, userId);


            await Clients.All.SendAsync("ReceiveMessage", "System", $"Welcome {userName}, has successfully connected to the chat!");
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string recipientId, string conversationId, string content) // Needs tests
        {
            var userName = Context.User!.Identity.Name;
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var recipientConnectionsId = await _usersConnectionManager.GetUserConnectionsId(recipientId);

            if(recipientConnectionsId.Count < 0)
            {
                throw new Exception();
            }

            await Clients.Clients(recipientConnectionsId).SendAsync("ReceiveMessage", userName, conversationId, content);

            var conversation = await _chatManager.GetOrCreateConversationAsync(userId, recipientId);
            var sender = await _userManager.FindByIdAsync(userId);

            var message = new Message()
            {
                ConversationId = Guid.Parse(conversationId),
                Conversation = conversation.Value,
                SenderId = userId,
                Sender = sender,
                Content = content,
            };

            await _chatManager.SaveMessageAsync(message);
        }

        public async Task GetOnlineUsers()
        {
            await Clients.Caller.SendAsync("ReceiveConnections", _usersConnectionManager.GetOnlineUsersIdAsync());
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Clients.All.SendAsync("ReceiveMessage", "System", $"{userName} has disconnected.");

            _usersConnectionManager.RemoveAsync(Context.ConnectionId, userId);

            await base.OnDisconnectedAsync(exception);
        }

    }
}
