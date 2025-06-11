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

        public ChatHub(IUsersConnectionService usersConnectionManager,
            IChatService chatService,
            UserManager<UserAccount> userManager,
            IMapper mapper)
        {
            _usersConnectionManager = usersConnectionManager;
            _chatService = chatService;
            _userManager = userManager;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User!.Identity.Name;
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _usersConnectionManager.AddUpdateAsync(Context.ConnectionId, Guid.Parse(userId));

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(Guid recipientId, Guid conversationId, string content) // Needs tests
        {
            var userName = Context.User!.Identity.Name;
            var userIdString = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userId = Guid.Parse(userIdString);

            var recipientConnectionsId = _usersConnectionManager.GetUserConnectionsId(recipientId);
            var senderConnectionsId = _usersConnectionManager.GetUserConnectionsId(userId);

            var conversation = await _chatService.GetOrCreateConversationAsync(userId, recipientId);
            var sender = await _userManager.FindByIdAsync(userIdString);

            if (sender == null)
            {
                return;
            }

            var message = new Message()
            {
                ConversationId = conversationId,
                Conversation = conversation.Value,
                SenderId = userId,
                Sender = sender,
                Content = content,
            };

            var messageDto = _mapper.Map<MessageDto>(message);

            if (recipientConnectionsId != null)
            {
                await Clients.Clients(recipientConnectionsId).ReceiveMessage(messageDto);
            }

            if (senderConnectionsId != null)
            {
                await Clients.Clients(senderConnectionsId).ReceiveMessage(messageDto);
            }

            await _chatService.SaveMessageAsync(message);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.User?.Identity?.Name;
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _usersConnectionManager.RemoveAsync(Context.ConnectionId, Guid.Parse(userId));

            await base.OnDisconnectedAsync(exception);
        }

    }
}
