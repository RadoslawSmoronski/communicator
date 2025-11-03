using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<FriendshipEntity> _dbSet;

        private readonly IMapper _mapper;

        public FriendshipRepository(DbContext context, IMapper mapper)
        {
            _context = context;
            _dbSet = _context.Set<FriendshipEntity>();
            _mapper = mapper;
        }

        public async Task<List<Friendship>> GetAllAsync(Guid userId)
        {
            var result = await _dbSet
                .Include(x => x.User1)
                .Include(x => x.User2)
                .Where(x => x.User1Id == userId || x.User2Id == userId)
                .ToListAsync();

            return _mapper.Map<List<Friendship>>(result);
        }


        public async Task<Friendship?> GetAsync(Guid friendshipId)
        {
            var result = await _dbSet
                .Include(x => x.User1)
                .Include(x => x.User2)
                .FirstOrDefaultAsync(x => x.Id == friendshipId);

            return _mapper.Map<Friendship>(result);
        }

        public async Task<bool> IsExistAsync(Guid user1Id, Guid user2Id)
            => await _dbSet.AnyAsync(x => 
                (x.User1Id == user1Id && x.User2Id == user2Id) ||
                (x.User1Id == user2Id && x.User2Id == user1Id));

        public async Task AddAsync(Friendship entity)
            => await _dbSet.AddAsync(_mapper.Map<FriendshipEntity>(entity));

        public async Task DeleteAsync(Guid id)
        {
            var elementToRemove = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
            _dbSet.Remove(elementToRemove!);
        }

        public void Update(Friendship entity)
            => _dbSet.Update(_mapper.Map<FriendshipEntity>(entity));
    }
}