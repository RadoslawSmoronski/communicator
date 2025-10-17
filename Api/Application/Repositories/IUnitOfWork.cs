using Domain.Entities;

namespace Application.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        public IRepository<RefreshToken> RefreshTokens { get; }
        public IFriendshipRepository Friendships { get; }
        public IFriendshipInvitationRepository FriendshipInvitations { get; }


        //Chat
        public IConversationRepository Conversations { get; }
        public IMessageRepository Messages { get; }
        Task<int> SaveAsync();
    }
}
