using Api.Data.Repository;
using Api.Models;

namespace Api.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<RefreshToken> _refreshTokens;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);

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
