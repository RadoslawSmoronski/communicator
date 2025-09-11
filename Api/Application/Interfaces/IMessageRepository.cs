using Domain.Entities;

namespace Application.Interfaces
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<IEnumerable<Message>> GetPagedMessagesFromMessageIdAsync(Guid ConversationId, Guid fromMessageId, int pageSize);
        Task<Message?> GetUserLastFriendMessageAsync(Guid ConversationId, Guid userId);
    }
}
