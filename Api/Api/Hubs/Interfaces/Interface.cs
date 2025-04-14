using Api.Models.Dtos.Chat;

namespace Api.Hubs.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(MessageDto message);
    }
}
