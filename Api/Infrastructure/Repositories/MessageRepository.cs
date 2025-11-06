using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<MessageEntity> _dbSet;

        private readonly IMapper _mapper;

        public MessageRepository(DbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = _context.Set<MessageEntity>();
            _mapper = mapper;
        }

        public async Task<IEnumerable<Message>> GetPagedMessagesFromMessageIdAsync(Guid conversationId, Guid fromMessageId, int pageSize)
        {
            var fromMessage = await _dbSet.FirstOrDefaultAsync(x => x.Id == fromMessageId);

            if (fromMessage is null)
                throw new KeyNotFoundException($"Message '{fromMessageId}' for conversation '{conversationId}' was not found.");

            if (fromMessage.ConversationId != conversationId)
                throw new InvalidOperationException($"Message '{fromMessageId}' does not belong to conversation '{conversationId}'.");

            var result = _dbSet
                .Where(x => x.ConversationId == conversationId && x.Timestamp < fromMessage.Timestamp)
                .OrderByDescending(x => x.Timestamp)
                .Take(pageSize);

            return _mapper.Map<List<Message>>(result);
        }

        public async Task<Message?> GetUserLastFriendMessageAsync(Guid conversationId, Guid userId)
        {
            var result = await _dbSet
                .Where(x => x.ConversationId == conversationId && x.SenderId != userId)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync();

            return _mapper.Map<Message?>(result);
        }

        public async Task AddAsync(Message entity)
            => await _dbSet.AddAsync(_mapper.Map<MessageEntity>(entity));

        public async Task DeleteAsync(Guid id)
        {
            var elementToRemove = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            _dbSet.Remove(elementToRemove!);
        }

        public void Update(Message entity)
            => _dbSet.Update(_mapper.Map<MessageEntity>(entity));
    }
}
