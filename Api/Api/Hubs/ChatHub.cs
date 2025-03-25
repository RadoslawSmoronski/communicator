using Api.Managers;
using Api.Managers.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
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
        
        public ChatHub(IUsersConnectionManager usersConnectionManager)
        {
            _usersConnectionManager = usersConnectionManager;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User!.Identity.Name;
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _usersConnectionManager.AddUpdateAsync(Context.ConnectionId, userId);


            await Clients.All.SendAsync("ReceiveMessage", "System", $"Welcome {userName}, has successfully connected to the chat!");
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string message)
        {
            var userName = Context.User!.Identity.Name;
            await Clients.All.SendAsync("ReceiveMessage", userName, message);
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
