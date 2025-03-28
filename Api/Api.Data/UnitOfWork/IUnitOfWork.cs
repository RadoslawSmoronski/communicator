using Api.Data.Repository;
using Api.Models;
using Api.Models.Chat;
using Api.Models.Friendship;

namespace Api.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        public IRepository<RefreshToken> RefreshTokens { get; }
        public IRepository<Friendship> Friendships { get; }
        public IRepository<FriendshipInvitation> FriendshipInvitations { get; }

        //Chat
        public IRepository<Conversation> Conversations { get; }
        public IRepository<Message> Messages { get; }
        Task<int> SaveAsync();
    }
}
