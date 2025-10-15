using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Identity;
using Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Shared.Result;

namespace Infrastructure.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly DbSet<ConversationEntity> _dbSet;
        private readonly IMapper _mapper;

        public ConversationRepository(DbContext context, IMapper mapper)
        {
            _dbSet = context.Set<ConversationEntity>();
            _mapper = mapper;
        }

        public async Task<Conversation?> GetConversationAsync(Guid user1Id, Guid user2Id)
        {
            var result = await _dbSet.FirstOrDefaultAsync(x =>
                (x.User1Id == user1Id && x.User2Id == user2Id) ||
                (x.User1Id == user2Id && x.User2Id == user1Id));

            return _mapper.Map<Conversation>(result);
        }

        public async Task AddAsync(Conversation conversation)
            => await _dbSet.AddAsync(_mapper.Map<ConversationEntity>(conversation));

        public async Task<List<Conversation>> GetUserAllAsync(Guid userId)
        {
            var conversations = _dbSet.Where(x => x.User1Id == userId || x.User2Id == userId);

            return _mapper.Map<List<Conversation>>(conversations);
        }

    }
}