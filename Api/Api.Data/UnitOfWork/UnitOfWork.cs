using Api.Data.Repository;
using Api.Data.Repository.Interfaces;
using Api.Models;
using Api.Models.Chat;
using Api.Models.Friendship;

namespace Api.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<RefreshToken> _refreshTokens;
        private IRepository<Friendship> _friendships;
        private IRepository<FriendshipInvitation> _friendshipInvitations;
        private IRepository<Conversation> _conversations;
        private IMessageRepository _messages;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);
        public IRepository<Friendship> Friendships => _friendships ??= new Repository<Friendship>(_context);
        public IRepository<FriendshipInvitation> FriendshipInvitations => _friendshipInvitations ??= new Repository<FriendshipInvitation>(_context);
        public IRepository<Conversation> Conversations => _conversations ??= new Repository<Conversation>(_context);
        public IMessageRepository Messages => _messages ??= new MessageRepository(_context);


        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
