using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FriendshipInvitationRepository : BaseRepository<FriendshipInvitation, FriendshipInvitationEntity>, IFriendshipInvitationRepository
    {
        public FriendshipInvitationRepository(DbContext context, IMapper mapper) : base(context, mapper){}

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
    }
}