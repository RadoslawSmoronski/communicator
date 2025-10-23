using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ConversationRepository : BaseRepository<Conversation, ConversationEntity>, IConversationRepository
    {
        public ConversationRepository(DbContext context, IMapper mapper) : base(context, mapper) {}

        public async Task<Conversation?> GetConversationByUsersIdAsync(Guid user1Id, Guid user2Id)
        {
            var result = await _dbSet.FirstOrDefaultAsync(x =>
                (x.User1Id == user1Id && x.User2Id == user2Id) ||
                (x.User1Id == user2Id && x.User2Id == user1Id));

            return _mapper.Map<Conversation>(result);
        }

        public List<Conversation> GetUserAll(Guid userId)
        {
            var conversations = _dbSet
                .Include(x => x.User1)
                .Include(x => x.User2)
                .Include(x => x.LastMessage)
                .Where(x => x.User1Id == userId || x.User2Id == userId);

            return _mapper.Map<List<Conversation>>(conversations);
        }

        public async Task<Conversation?> GetConversationByIdAsync(Guid conversationId)
        {
            var result = await _dbSet.FirstOrDefaultAsync(x => x.Id == conversationId);
            return _mapper.Map<Conversation>(result);
        }

    }
}