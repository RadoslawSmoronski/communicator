using ChatCommunicator.Contracts.Dtos.Chat;

namespace ChatCommunicator.Application.Hubs.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(MessageDto message);
        Task MessageRead(Guid messageId);
        Task FriendConnect(Guid friendId);
        Task FriendDisconnect(Guid friendId);
    }
}
