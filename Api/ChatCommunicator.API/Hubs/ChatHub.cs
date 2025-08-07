using ChatCommunicator.Contracts.Dtos.Chat;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using ChatCommunicator.Application.Hubs.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Models.Chat;

namespace ChatCommunicator.Application.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IUsersConnectionService _usersConnectionManager;
        private readonly IChatService _chatService;
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IUsersConnectionService usersConnectionManager,
            IChatService chatService,
            UserManager<UserAccount> userManager,
            IMapper mapper,
            ILogger<ChatHub> logger)
        {
            _usersConnectionManager = usersConnectionManager;
            _chatService = chatService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdString, out var userId))
            {
                _logger.LogInformation("User connected. UserId: {UserId}, UserName: {UserName}, ConnectionId: {ConnectionId}", userId, userName, Context.ConnectionId);

                await _usersConnectionManager.AddUpdateAsync(Context.ConnectionId, userId);
            }
            else
            {
                _logger.LogWarning("User connected with invalid or missing UserId. ConnectionId: {ConnectionId}", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(Guid recipientId, Guid conversationId, string content) // TODO: Needs tests and finish UNDER section
        {
            var userName = Context.User?.Identity?.Name;
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                _logger.LogWarning("SendMessage called with invalid user ID. ConnectionId: {ConnectionId}", Context.ConnectionId);
                return;
            }

            var result = await _chatService.SendMessageAsync(userId, conversationId, content);


            // UNDER SECTION <- do something with this
            if(result.IsSuccess)
            {
                var recipientConnectionsId = _usersConnectionManager.GetUserConnectionsId(recipientId);
                var senderConnectionsId = _usersConnectionManager.GetUserConnectionsId(userId);

                try
                {
                    if (recipientConnectionsId != null)
                    {
                        await Clients.Clients(recipientConnectionsId).ReceiveMessage(result.Value);
                    }

                    if (senderConnectionsId != null)
                    {
                        await Clients.Clients(senderConnectionsId).ReceiveMessage(result.Value);
                    }
                }
                catch (Exception ex)
                {
                    
                }
            }

            //UNDER SECTION
        }

        public async Task ReadMessage(Guid recipientId, Guid conversationId)
        {
            var userName = Context.User?.Identity?.Name;
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                return;
            }
            var recipientConnectionsId = _usersConnectionManager.GetUserConnectionsId(recipientId);
            var senderConnectionsId = _usersConnectionManager.GetUserConnectionsId(userId);

            var result = await _chatService.SetAndGetUserLastReadMessageAsync(userId, conversationId);

            if (result.IsSuccess)
            {
                if (recipientConnectionsId != null)
                {
                    await Clients.Clients(recipientConnectionsId).MessageRead(result.Value);
                }
            }

        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdString, out var userId))
            {
                _logger.LogInformation("User disconnected. UserId: {UserId}, UserName: {UserName}, ConnectionId: {ConnectionId}, Exception: {Exception}",
                    userId, userName, Context.ConnectionId, exception?.Message);

                await _usersConnectionManager.RemoveAsync(Context.ConnectionId, userId);
            }
            else
            {
                _logger.LogWarning("User disconnected with invalid or missing UserId. ConnectionId: {ConnectionId}, Exception: {Exception}", Context.ConnectionId, exception?.Message);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
