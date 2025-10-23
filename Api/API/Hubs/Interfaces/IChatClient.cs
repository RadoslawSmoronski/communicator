using API.DTOs;
using Application.DTOs;

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
