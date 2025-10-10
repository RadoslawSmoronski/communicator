using Application.Repositories;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class FriendshipInvitationRepository : IFriendshipInvitationRepository
    {
        private readonly DbSet<FriendshipInvitationEntity> _dbSet;

        public FriendshipInvitationRepository(DbContext context)
        {
            _dbSet = context.Set<FriendshipInvitationEntity>();
        }

        public async Task<IReadOnlyList<FriendshipInvitation>> GetAllAsync(Guid userId)
        {
            var entities = await _dbSet
                .Include(x => x.SenderUser)
                .Include(x => x.RecipientUser)
                .Where(x => x.RecipientId == userId)
                .ToListAsync();

            return entities.Select(entity => new FriendshipInvitation
            {
                Id = entity.Id,
                SenderId = entity.SenderId,
                RecipientId = entity.RecipientId,
                SenderUser = new User() { Id = entity.SenderUser.Id, UserName = entity.SenderUser.UserName!, AvatarUrl = entity.SenderUser.AvatarUrl},
                RecipientUser = new User() { Id = entity.RecipientUser.Id, UserName = entity.RecipientUser.UserName!, AvatarUrl = entity.RecipientUser.AvatarUrl },
                CreatedAt = entity.CreatedAt
            }).ToList();
        }

        public async Task AddAsync(FriendshipInvitation friendshipInvitation)
        {
            var entity = new FriendshipInvitationEntity()
            {
                Id = friendshipInvitation.Id,
                SenderId = friendshipInvitation.SenderId,
                RecipientId = friendshipInvitation.RecipientId,
                CreatedAt = friendshipInvitation.CreatedAt
            };

            await _dbSet.AddAsync(entity);
        }


        public async Task<FriendshipInvitation?> GetById(Guid id)
        {
            var entity = await _dbSet
                .Include(x => x.SenderUser)
                .Include(x => x.RecipientUser)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return null;

            return new FriendshipInvitation
            {
                Id = entity.Id,
                SenderId = entity.SenderId,
                RecipientId = entity.RecipientId,
                SenderUser = new User
                {
                    Id = entity.SenderUser.Id,
                    UserName = entity.SenderUser.UserName!,
                    AvatarUrl = entity.SenderUser.AvatarUrl
                },
                RecipientUser = new User
                {
                    Id = entity.RecipientUser.Id,
                    UserName = entity.RecipientUser.UserName!,
                    AvatarUrl = entity.RecipientUser.AvatarUrl
                },
                CreatedAt = entity.CreatedAt
            };
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