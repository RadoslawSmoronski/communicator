using Application.Contracts.Chat;

namespace API.Hubs.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(MessageReceivedEvent message);
        Task MessageRead(MessageReadEvent messageRead);
        Task FriendConnect(Guid friendId);
        Task FriendDisconnect(Guid friendId);
    }
}
