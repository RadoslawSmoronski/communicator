using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Chat;
using ChatCommunicator.Contracts.Dtos.Chat;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using ChatCommunicator.Application.Hubs.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;

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

        public async Task SendMessage(Guid recipientId, Guid conversationId, string content) // Needs tests
        {
            var userName = Context.User?.Identity?.Name;
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                _logger.LogWarning("SendMessage called with invalid user ID. ConnectionId: {ConnectionId}", Context.ConnectionId);
                return;
            }

            _logger.LogInformation("User {UserId} sending message to {RecipientId} in conversation {ConversationId}", userId, recipientId, conversationId);

            var recipientConnectionsId = _usersConnectionManager.GetUserConnectionsId(recipientId);
            var senderConnectionsId = _usersConnectionManager.GetUserConnectionsId(userId);

            var conversationResult = await _chatService.GetOrCreateConversationAsync(userId, recipientId);
            if (conversationResult.Error != null)
            {
                _logger.LogError("Failed to get or create conversation between {UserId} and {RecipientId}: {Error}", userId, recipientId, conversationResult.Error.Description);
                return;
            }

            var sender = await _userManager.FindByIdAsync(userIdString);
            if (sender == null)
            {
                _logger.LogWarning("SendMessage failed: sender user not found. UserId: {UserId}", userId);
                return;
            }

            var message = new Message()
            {
                ConversationId = conversationId,
                Conversation = conversationResult.Value,
                SenderId = userId,
                Sender = sender,
                Content = content,
            };

            var messageDto = _mapper.Map<MessageDto>(message);

            try
            {
                if (recipientConnectionsId != null)
                {
                    await Clients.Clients(recipientConnectionsId).ReceiveMessage(messageDto);
                    _logger.LogDebug("Message sent to recipient connections: {RecipientConnectionsCount}", recipientConnectionsId.Count);
                }

                if (senderConnectionsId != null)
                {
                    await Clients.Clients(senderConnectionsId).ReceiveMessage(messageDto);
                    _logger.LogDebug("Message sent to sender connections: {SenderConnectionsCount}", senderConnectionsId.Count);
                }

                await _chatService.SaveMessageAsync(message);
                _logger.LogInformation("Message saved to database. ConversationId: {ConversationId}, SenderId: {SenderId}", conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending or saving message. UserId: {UserId}, RecipientId: {RecipientId}", userId, recipientId);
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
