using Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Data.IRepository
{
    public interface IRefreshTokenRepository
    {
        Task<bool> IsTokenValidAsync(string token);
        Task SaveTokenAsync(RefreshToken refreshToken);
        Task DeleteTokenAsync(string token);
        Task<string?> GetRefreshTokenAsyncByUserId(string userId);
    }
}
