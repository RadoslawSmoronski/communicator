using ChatCommunicator.Infrastructure.Models.Chat;

namespace ChatCommunicator.Infrastructure.Repository.Interfaces
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<IEnumerable<Message>> GetPagedMessagesFromMessageIdAsync(Guid ConversationId, Guid fromMessageId, int pageSize);
    }
}
