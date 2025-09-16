using Domain.Entities;

namespace Application.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        public IRepository<RefreshToken> RefreshTokens { get; }
        public IRepository<Friendship> Friendships { get; }
        public IRepository<FriendshipInvitation> FriendshipInvitations { get; }

        //Chat
        public IRepository<Conversation> Conversations { get; }
        public IMessageRepository Messages { get; }
        Task<int> SaveAsync();
    }
}
