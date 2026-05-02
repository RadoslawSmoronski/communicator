using Domain.Entities;

namespace Application.Repositories
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetPagedMessagesFromMessageIdAsync(Guid ConversationId, Guid fromMessageId, int pageSize);
        Task<Message?> GetUserLastFriendMessageAsync(Guid ConversationId, Guid userId);

        Task AddAsync(Message entity);
        Task DeleteAsync(Guid id);
        void Update(Message entity);
    }
}
