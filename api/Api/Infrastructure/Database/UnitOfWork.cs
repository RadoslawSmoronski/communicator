using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Repositories;

namespace Infrastructure.Database
{
    public class UnitOfWork(ApplicationDbContext context, IMapper mapper) : IUnitOfWork
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        private IRepository<RefreshToken>? _refreshTokens;
        private IFriendshipRepository? _friendships;
        private IFriendshipInvitationRepository? _friendshipInvitations;
        private IConversationRepository? _conversations;
        private IMessageRepository? _messages;

        public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);
        public IFriendshipRepository Friendships => _friendships ??= new FriendshipRepository(_context, _mapper);
        public IFriendshipInvitationRepository FriendshipInvitations => _friendshipInvitations ??= new FriendshipInvitationRepository(_context, _mapper);
        public IConversationRepository Conversations => _conversations ??= new ConversationRepository(_context, _mapper);
        public IMessageRepository Messages => _messages ??= new MessageRepository(_context, _mapper);

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
