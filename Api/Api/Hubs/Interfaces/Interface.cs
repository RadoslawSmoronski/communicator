using ChatCommunicator.Contracts.Dtos.Chat;

namespace ChatCommunicator.Hubs.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(MessageDto message);
    }
}
