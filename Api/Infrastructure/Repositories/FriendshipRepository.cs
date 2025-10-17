using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FriendshipRepository : BaseRepository<Friendship, FriendshipEntity>, IFriendshipRepository
    {
        public FriendshipRepository(DbContext context, IMapper mapper) : base(context, mapper) { }

        public async Task<List<Friendship>> GetAllAsync(Guid userId)
        {
            var result = await _dbSet
                .Include(x => x.User1)
                .Include(x => x.User2)
                .Where(x => x.User1Id == userId || x.User2Id == userId)
                .ToListAsync();

            return _mapper.Map<List<Friendship>>(result);
        }


        public async Task<Friendship?> Get(Guid friendshipId)
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

    }
}