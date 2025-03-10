using Api.Data.Repository;
using Api.Models;
using Api.Models.Friendship;

namespace Api.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        public IRepository<Friendship> Friendships { get; }
        public IRepository<FriendshipInvitation> FriendshipInvitations { get; }
        Task<int> SaveAsync();
    }
}
