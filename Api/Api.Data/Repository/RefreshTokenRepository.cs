using Api.Data.IRepository;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Api.Data.Repository
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTokenValidAsync(string token)
        {
            return await _context.RefreshTokens
                .AnyAsync(rt => rt.Token == token && rt.Expiration > DateTime.UtcNow);
        }

        public async Task SaveTokenAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .SingleOrDefaultAsync(rt => rt.Token == token);

            if(refreshToken != null)
            {
                _context.RefreshTokens.Remove(refreshToken);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string?> GetRefreshTokenByUserIdAsync(string userId)
        {
            return await _context.RefreshTokens
                   .Where(rt => rt.UserId == userId)
                   .Select(rt => rt.Token)
                   .FirstOrDefaultAsync();
        }

        public async Task<string?> GetUserIdByRefreshTokenAsync(string refreshToken)
        {
            return await _context.RefreshTokens
                   .Where(rt => rt.Token == refreshToken)
                   .Select(rt => rt.UserId)
                   .FirstOrDefaultAsync();
        }

        public async Task<int> RemoveExpiredRefreshTokensAsync()
        {
            var expiredRefreshTokens = await _context.RefreshTokens
                .Where(rt => rt.Expiration < DateTime.UtcNow)
                .ToListAsync();

            if (expiredRefreshTokens.Count > 0)
            {
                _context.RefreshTokens.RemoveRange(expiredRefreshTokens);
                await _context.SaveChangesAsync();
            }

            return expiredRefreshTokens.Count;
        }
    }
}
