using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<Message> _dbSet;

        public MessageRepository(DbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<Message>();
        }

        public async Task<IEnumerable<Message>> GetPagedMessagesFromMessageIdAsync(Guid conversationId, Guid fromMessageId, int pageSize)
        {
            var fromMessage = await _dbSet.FirstOrDefaultAsync(x => x.Id == fromMessageId);

            if (fromMessage == null)
                throw new Exception();

            return _dbSet
                .Where(x => x.ConversationId == conversationId && x.Timestamp < fromMessage.Timestamp)
                .OrderByDescending(x => x.Timestamp)
                .Take(pageSize);
        }

        public async Task<Message?> GetUserLastFriendMessageAsync(Guid conversationId, Guid userId)
        {
            return await _dbSet
                .Where(x => x.ConversationId == conversationId && x.SenderId != userId)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();
        }
    }
}
