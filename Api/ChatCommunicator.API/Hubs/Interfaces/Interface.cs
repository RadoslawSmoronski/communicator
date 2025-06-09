using ChatCommunicator.Contracts.Dtos.Chat;

namespace ChatCommunicator.API.Hubs.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(MessageDto message);
    }
}
