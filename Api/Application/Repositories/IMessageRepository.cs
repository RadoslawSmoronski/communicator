using Domain.Entities;

namespace Application.Repositories
{
    public interface IMessageRepository : IBaseRepository<Message>
    {
        Task<IEnumerable<Message>> GetPagedMessagesFromMessageIdAsync(Guid ConversationId, Guid fromMessageId, int pageSize);
        Task<Message?> GetUserLastFriendMessageAsync(Guid ConversationId, Guid userId);
    }
}
