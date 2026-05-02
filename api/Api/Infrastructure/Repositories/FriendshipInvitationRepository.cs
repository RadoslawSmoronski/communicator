using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FriendshipInvitationRepository : IFriendshipInvitationRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<FriendshipInvitationEntity> _dbSet;

        private readonly IMapper _mapper;

        public FriendshipInvitationRepository(DbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = _context.Set<FriendshipInvitationEntity>();
            _mapper = mapper;
        }

        public async Task<List<FriendshipInvitation>> GetAllAsync(Guid userId)
        {
            var entities = await _dbSet
                .Include(x => x.SenderUser)
                .Include(x => x.RecipientUser)
                .Where(x => x.RecipientId == userId || x.SenderId == userId)
                .ToListAsync();

            return entities.Select(entity => _mapper.Map<FriendshipInvitation>(entity)).ToList();
        }

        public async Task<FriendshipInvitation?> GetById(Guid id)
        {
            var entity = await _dbSet
                .Include(x => x.SenderUser)
                .Include(x => x.RecipientUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return null;

            return _mapper.Map<FriendshipInvitation>(entity);
        }

        public async Task<bool> IsExistAsync(Guid user1Id, Guid user2Id)
            => await _dbSet.AnyAsync(x => x.SenderId == user1Id && x.RecipientId == user2Id ||
            x.SenderId == user2Id && x.RecipientId == user1Id);

        public async Task AddAsync(FriendshipInvitation entity)
            => await _dbSet.AddAsync(_mapper.Map<FriendshipInvitationEntity>(entity));

        public async Task DeleteAsync(Guid id)
        {
            var elementToRemove = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            _dbSet.Remove(elementToRemove!);
        }

        public void Update(FriendshipInvitation entity)
            => _dbSet.Update(_mapper.Map<FriendshipInvitationEntity>(entity));
    }
}