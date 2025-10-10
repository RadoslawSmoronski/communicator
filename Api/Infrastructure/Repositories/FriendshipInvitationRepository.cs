using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FriendshipInvitationRepository : IFriendshipInvitationRepository
    {
        private readonly DbSet<FriendshipInvitationEntity> _dbSet;
        private readonly IMapper _mapper;

        public FriendshipInvitationRepository(DbContext context, IMapper mapper)
        {
            _dbSet = context.Set<FriendshipInvitationEntity>();
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<FriendshipInvitation>> GetAllAsync(Guid userId)
        {
            var entities = await _dbSet
                .Include(x => x.SenderUser)
                .Include(x => x.RecipientUser)
                .Where(x => x.RecipientId == userId)
                .ToListAsync();

            return entities.Select(entity => _mapper.Map<FriendshipInvitation>(entity)).ToList();
        }

        public async Task AddAsync(FriendshipInvitation friendshipInvitation)
             => await _dbSet.AddAsync(_mapper.Map<FriendshipInvitationEntity>(friendshipInvitation));



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

        public async Task DeleteAsync(Guid invitationId)
        {
            var entity = await _dbSet.FindAsync(invitationId);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public async Task<bool> IsExistAsync(Guid user1Id, Guid user2Id)
            => await _dbSet.AnyAsync(x => x.SenderId == user1Id && x.RecipientId == user2Id ||
            x.SenderId == user2Id && x.RecipientId == user1Id);
    }
}