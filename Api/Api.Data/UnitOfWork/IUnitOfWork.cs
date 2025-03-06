using Api.Data.Repository;
using Api.Models;

namespace Api.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        public IRepository<RefreshToken> RefreshTokens { get; }
        Task<int> SaveAsync();
    }
}
