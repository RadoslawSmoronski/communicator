using AutoMapper;
using ChatCommunicator.Application.Hubs.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatCommunicator.Application.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        private readonly IUsersConnectionService _usersConnectionManager;
        private readonly IChatService _chatService;
        private readonly IFriendsService _friendsService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IUsersConnectionService usersConnectionManager,
            IChatService chatService,
            UserManager<UserAccount> userManager,
            IMapper mapper,
            IFriendsService friendsService,
            ILogger<ChatHub> logger)
        {
            _usersConnectionManager = usersConnectionManager;
            _chatService = chatService;
            _friendsService = friendsService;
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

                var onlineFriends = await _friendsService.GetUserOnlineFriendsConnectionsIdAsync(userId);

                if(onlineFriends.IsSuccess)
                {
                    await Clients.Clients(onlineFriends.Value).FriendConnect(userId);
                }
            }
            else
            {
                _logger.LogWarning("User connected with invalid or missing UserId. ConnectionId: {ConnectionId}", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(Guid recipientId, Guid conversationId, string content)
        {
            if (!TryGetUserId(out Guid userId))
            {
                _logger.LogWarning("SendMessage called with invalid user ID. ConnectionId: {ConnectionId}", Context.ConnectionId);
                return;
            }

            var result = await _chatService.SendMessageAsync(userId, conversationId, content);

            if (result.IsSuccess)
            {
                await NotifyClients(userId, recipientId,
                    (clients, connections) => clients.Clients(connections).ReceiveMessage(result.Value),
                    "sending message");
            }
            else
            {
                _logger.LogWarning("Failed to send message. SenderId: {SenderId}, RecipientId: {RecipientId}, ConversationId: {ConversationId}, Error: {ErrorMessage}",
                    userId, recipientId, conversationId, result.Error?.Description ?? "Unknown error");
            }
        }

        public async Task ReadMessage(Guid recipientId, Guid conversationId)
        {
            if (!TryGetUserId(out Guid userId))
            {
                _logger.LogWarning("ReadMessage called with invalid user ID. ConnectionId: {ConnectionId}", Context.ConnectionId);
                return;
            }

            var result = await _chatService.SetAndGetUserLastReadMessageAsync(userId, conversationId);

            if (result.IsSuccess)
            {
                await NotifyClients(userId, recipientId,
                    (clients, connections) => clients.Clients(connections).MessageRead(new MessageReadDto { MessageId = result.Value, ConversationId = conversationId}),
                    "reading message");
            }
            else
            {
                _logger.LogWarning("Failed to set message as read. UserId: {UserId}, ConversationId: {ConversationId}, Error: {ErrorMessage}",
                    userId, conversationId, result.Error?.Description ?? "Unknown error");
            }

        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (TryGetUserId(out Guid userId))
            {
                _logger.LogInformation("User disconnected. UserId: {UserId}, ConnectionId: {ConnectionId}, Exception: {Exception}",
                    userId, Context.ConnectionId, exception?.Message);

                await _usersConnectionManager.RemoveAsync(Context.ConnectionId, userId);

                var isUserDisconnect = await IsUserDisconnect(userId);

                if (isUserDisconnect.IsSuccess)
                {
                    await Clients.Clients(isUserDisconnect.Value).FriendDisconnect(userId);
                }
            }
            else
            {
                _logger.LogWarning("User disconnected with invalid or missing UserId. ConnectionId: {ConnectionId}, Exception: {Exception}", Context.ConnectionId, exception?.Message);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private bool TryGetUserId(out Guid userId)
        {
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdString, out userId);
        }

        private async Task NotifyClients(Guid userId, Guid recipientId, Func<IHubClients<IChatClient>, IReadOnlyList<string>, Task> notificationAction, string operationDescription)
        {
            var recipientConnectionsId = _usersConnectionManager.GetUserConnectionsId(recipientId);
            var senderConnectionsId = _usersConnectionManager.GetUserConnectionsId(userId);

            if (recipientConnectionsId != null)
            {
                try
                {
                    await notificationAction(Clients, recipientConnectionsId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error {OperationDescription} to recipient {RecipientId}. ConnectionIds: {ConnectionIds}",
                        operationDescription, recipientId, string.Join(", ", recipientConnectionsId));
                }
            }

            if (senderConnectionsId != null)
            {
                try
                {
                    await notificationAction(Clients, senderConnectionsId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error {OperationDescription} to sender {SenderId}. ConnectionIds: {ConnectionIds}",
                        operationDescription, userId, string.Join(", ", senderConnectionsId));
                }
            }
        }
    
        private async Task<ResultT<List<string>>> IsUserDisconnect(Guid userId)
        {
            var isOnline = await _usersConnectionManager.IsUserOnlineAsync(userId);

            if (isOnline)
            {
                return ResultT<List<string>>.Failure(
                    Error.Failure("UserStillOnline", "User is still online."));
            }

            var friendsConnectionsResult = await _friendsService.GetUserOnlineFriendsConnectionsIdAsync(userId);
            if (!friendsConnectionsResult.IsSuccess)
            {
                return ResultT<List<string>>.Failure(friendsConnectionsResult.Error!);
            }

            return friendsConnectionsResult.Value;
        }
    
    }
}
