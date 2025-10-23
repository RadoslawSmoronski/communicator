using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Infrastructure.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<ConversationEntity> _dbSet;

        private readonly IMapper _mapper;

        public ConversationRepository(DbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = _context.Set<ConversationEntity>();
            _mapper = mapper;
        }

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

        public async Task AddAsync(Conversation entity)
        => await _dbSet.AddAsync(_mapper.Map<ConversationEntity>(entity));

        public async Task DeleteAsync(Guid id)
        {
            var elementToRemove = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            _dbSet.Remove(elementToRemove!);
        }

        public async Task UpdateAsync(Conversation entity)
        {
            var elementToUpdate = await _dbSet.FirstOrDefaultAsync(x => x.Id == entity.Id);

            elementToUpdate!.User1Id = entity.User1Id;
            elementToUpdate.User2Id = entity.User2Id;
            elementToUpdate.LastMessageId = entity.LastMessageId;
            elementToUpdate.User1LastReadMessageId = entity.User1LastReadMessageId;
            elementToUpdate.User2LastReadMessageId = entity.User2LastReadMessageId;
            elementToUpdate.CreatedAt = entity.CreatedAt;
            elementToUpdate.LastMessageTime = entity.LastMessageTime;

            _dbSet.Update(elementToUpdate);
        }

    }
}