using Api.Data.Repository.Interfaces;
using Api.Models.Chat;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Data.Repository
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

        public async Task<List<Message>> GetPagedMessagesFromIdAsync(Guid conversationId, Guid fromMessageId, int pageSize)
        {
            var fromMessage = await _dbSet.FirstOrDefaultAsync(x => x.Id == fromMessageId);

            if (fromMessage == null)
                throw new Exception();

            return await _dbSet
                .Where(x => x.ConversationId == conversationId && x.Timestamp <= fromMessage.Timestamp)
                .OrderByDescending(x => x.Timestamp)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
