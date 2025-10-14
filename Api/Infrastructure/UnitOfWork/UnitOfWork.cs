using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Repositories;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<RefreshToken>? _refreshTokens;
        private IFriendshipRepository? _friendships;
        private IFriendshipInvitationRepository? _friendshipInvitations;
        private IRepository<Conversation>? _conversations;
        private IMessageRepository? _messages;
        private IMapper _mapper;

        public UnitOfWork(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);
        public IFriendshipRepository Friendships => _friendships ??= new FriendshipRepository(_context, _mapper);
        public IFriendshipInvitationRepository FriendshipInvitations => _friendshipInvitations ??= new FriendshipInvitationRepository(_context, _mapper);
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
