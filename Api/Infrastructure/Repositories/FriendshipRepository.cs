using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly DbSet<FriendshipEntity> _dbSet;
        private readonly IMapper _mapper;

        public FriendshipRepository(DbContext context, IMapper mapper)
        {
            _dbSet = context.Set<FriendshipEntity>();
            _mapper = mapper;
        }

        public async Task<List<Friendship>> GetAllAsync(Guid userId)
        {
            var result = await _dbSet
                .Include(x => x.User1)
                .Include(x => x.User2)
                .Where(x => x.User1Id == userId || x.User2Id == userId)
                .ToListAsync();

            return result.Select(entity => _mapper.Map<Friendship>(result)).ToList();
        }

        public async Task<Friendship?> Get(Guid friendshipId)
        {
            var result = await _dbSet
                .Include(x => x.User1)
                .Include(x => x.User2)
                .FirstOrDefaultAsync(x => x.Id == friendshipId);

            return _mapper.Map<Friendship>(result);
        }

        public async Task AddAsync(Friendship friendship)
             => await _dbSet.AddAsync(_mapper.Map<FriendshipEntity>(friendship));


        public async Task<bool> IsExistAsync(Guid user1Id, Guid user2Id)
            => await _dbSet.AnyAsync(x => 
                (x.User1Id == user1Id && x.User2Id == user2Id) ||
                (x.User1Id == user2Id && x.User2Id == user1Id));

        public async Task DeleteAsync(Guid friendshipId)
        {
            var friendship = await _dbSet.FirstAsync(x => x.Id == friendshipId);
            _dbSet.Remove(friendship);
        }
    }
}