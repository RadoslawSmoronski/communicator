using ChatCommunicator.Infrastructure.Repository;
using ChatCommunicator.Infrastructure.Repository.Interfaces;
using ChatCommunicator.Contracts;
using ChatCommunicator.Infrastructure.Models.Friendship;
using ChatCommunicator.Infrastructure.Models.Chat;

namespace ChatCommunicator.Infrastructure.UnitOfWork
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
