using Api.Data.IRepository;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _context.RefreshTokens.Add(refreshToken);
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

        public async Task<string?> GetRefreshTokenAsyncByUserId(string userId)
        {
            var refreshToken = await _context.RefreshTokens
                                             .Where(rt => rt.UserId == userId)
                                             .Select(rt => rt.Token)
                                             .FirstOrDefaultAsync();

            return refreshToken;
        }
    }
}
