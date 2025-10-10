using Application.Repositories;
using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Repositories;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<RefreshToken>? _refreshTokens;
        private IRepository<Friendship>? _friendships;
        private IFriendshipInvitationRepository? _friendshipInvitations;
        private IRepository<Conversation>? _conversations;
        private IMessageRepository? _messages;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);
        public IRepository<Friendship> Friendships => _friendships ??= new Repository<Friendship>(_context);
        public IFriendshipInvitationRepository FriendshipInvitations => _friendshipInvitations ??= new FriendshipInvitationRepository(_context);
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
