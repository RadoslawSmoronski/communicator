using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalRJWTServer.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            string user = Context.User?.Identity?.Name ?? "Guest";
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Welcome {user}, you are successfully connected to the chat!");
            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string user, string message)
        {
            string user2 = Context.User?.Identity?.Name ?? "Guest";
            await Clients.All.SendAsync("ReceiveMessage", user2, message);
        }

    }
}
