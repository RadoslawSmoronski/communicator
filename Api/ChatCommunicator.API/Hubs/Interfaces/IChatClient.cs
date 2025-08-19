using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Chat;

namespace ChatCommunicator.Application.Hubs.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(MessageDto message);
        Task MessageRead(MessageReadDto messageRead);
        Task FriendConnect(Guid friendId);
        Task FriendDisconnect(Guid friendId);
    }
}
