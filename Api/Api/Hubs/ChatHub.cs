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
        private ConcurrentDictionary<string, string> _usersOnline = new ConcurrentDictionary<string, string>();


        public override async Task OnConnectedAsync()
        {
            var userName = Context.User!.Identity.Name;
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _usersOnline.TryAdd(Context.ConnectionId, userId);


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
            var result = _usersOnline.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToList()
            );

            await Clients.Caller.SendAsync("ReceiveConnections", result);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_usersOnline.TryRemove(Context.ConnectionId, out var userId))
            {
                var userName = Context.User?.Identity?.Name;
                await Clients.All.SendAsync("ReceiveMessage", "System", $"{userName} has disconnected.");
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}
