using ChatCommunicator.Contracts.Dtos.Chat;

namespace ChatCommunicator.Application.Hubs.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(MessageDto message);
        Task MessageRead(Guid messageId);
    }
}
