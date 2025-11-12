using API.Hubs.Interfaces;
using Application.Common.Interfaces;
using Application.Contracts.Chat;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Shared.Result;
using System.Security.Claims;

namespace API.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub(
        IUsersConnectionService usersConnectionManager,
        ILogger<ChatHub> logger,
        IFriendshipService friendshipService,
        IMessageService messageService
    ) : Hub<IChatClient>
    {
        private readonly IUsersConnectionService _usersConnectionService = usersConnectionManager;
        private readonly ILogger<ChatHub> _logger = logger;
        private readonly IFriendshipService _friendshipService = friendshipService;
        private readonly IMessageService _messageService = messageService;

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User?.Identity?.Name;
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdString, out var userId))
            {
                _logger.LogInformation("User connected. UserId: {UserId}, UserName: {UserName}, ConnectionId: {ConnectionId}", userId, userName, Context.ConnectionId);

                await _usersConnectionService.AddUpdateAsync(Context.ConnectionId, userId);

                var onlineFriends = await GetUserOnlineFriendsConnectionsIdAsync(userId);

                if (onlineFriends.IsSuccess)
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

            var result = await _messageService.SendMessageAsync(userId, conversationId, content);

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

            var result = await _messageService.SetAndGetUserLastReadMessageAsync(userId, conversationId);

            if (result.IsSuccess)
            {
                await NotifyClients(null, recipientId,
                    (clients, connections) => clients.Clients(connections).MessageRead(new MessageReadEvent(MessageId: result.Value, ConversationId: conversationId )),
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

                await _usersConnectionService.RemoveAsync(Context.ConnectionId, userId);

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

        private async Task NotifyClients(Guid? userId, Guid? recipientId, Func<IHubClients<IChatClient>, IReadOnlyList<string>, Task> notificationAction, string operationDescription)
        {
            List<string>? recipientConnectionsId = null;
            List<string>? senderConnectionsId = null;

            if (userId != null)
            {
                senderConnectionsId = _usersConnectionService.GetUserConnectionsId(userId.Value);
            }

            if (recipientId != null)
            {
                recipientConnectionsId = _usersConnectionService.GetUserConnectionsId(recipientId.Value);
            }

            await NotifyConnections(recipientConnectionsId, notificationAction, operationDescription);
            await NotifyConnections(senderConnectionsId, notificationAction, operationDescription);
        }

        private async Task NotifyConnections(List<string>? connectionsId, Func<IHubClients<IChatClient>, IReadOnlyList<string>, Task> notificationAction, string operationDescription)
        {
            if (connectionsId != null)
            {
                try
                {
                    await notificationAction(Clients, connectionsId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during {OperationDescription}. ConnectionIds: {ConnectionIds}",
                        operationDescription, string.Join(", ", connectionsId));
                }
            }
        }

        private async Task<Result<List<string>>> IsUserDisconnect(Guid userId)
        {
            var isOnline = await _usersConnectionService.IsUserOnlineAsync(userId);

            if (isOnline)
            {
                _logger.LogInformation("User still online, skipping disconnect notification. UserId: {UserId}", userId);
                return Error.Failure("UserStillOnline", "User is still online.");
            }

            var friendsConnectionsResult = await GetUserOnlineFriendsConnectionsIdAsync(userId);
            if (!friendsConnectionsResult.IsSuccess)
            {
                _logger.LogWarning(
                    "Failed to get online friends' connections for disconnect notification. UserId: {UserId}, Error: {Code} - {Description}",
                    userId,
                    friendsConnectionsResult.Error?.Code,
                    friendsConnectionsResult.Error?.Description
                );

                return Result<List<string>>.Failure(friendsConnectionsResult.Error!);
            }

            return friendsConnectionsResult.Value;
        }

        private async Task<Result<List<string>>> GetUserOnlineFriendsConnectionsIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("Invalid userId provided when retrieving online friends connections. ConnectionId: {ConnectionId}", Context.ConnectionId);
                return Error.Validation("InvalidUserId", "The provided userId is empty.");
            }

            try
            {
                var friendsResult = await _friendshipService.GetUserFriendAsync(userId);
                if (!friendsResult.IsSuccess)
                {
                    _logger.LogWarning(
                        "GetUserFriendAsync failed. UserId: {UserId}. Error: {Code} - {Description}",
                        userId,
                        friendsResult.Error?.Code,
                        friendsResult.Error?.Description
                    );

                    return friendsResult.Error!;
                }

                var onlineFriendsConnections = new List<string>();

                foreach (var friend in friendsResult.Value)
                {
                    if (await _usersConnectionService.IsUserOnlineAsync(friend.Id))
                    {
                        var connections = _usersConnectionService.GetUserConnectionsId(friend.Id);
                        if (connections is { Count: > 0 })
                        {
                            onlineFriendsConnections.AddRange(connections);
                        }
                    }
                }

                _logger.LogDebug("Collected {Count} online friend connection(s) for user {UserId}.", onlineFriendsConnections.Count, userId);

                return onlineFriendsConnections;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving online friends connections. UserId: {UserId}", userId);
                return Error.Failure("OnlineFriendsConnectionsFetchFailed", "An unexpected error occurred while retrieving online friends connections.");
            }
        }

    }
}
