using ChatCommunicator.Data.Repository;
using ChatCommunicator.Data.Repository.Interfaces;
using ChatCommunicator.Models;
using ChatCommunicator.Models.Chat;
using ChatCommunicator.Models.Friendship;

namespace ChatCommunicator.Data.UnitOfWork
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
