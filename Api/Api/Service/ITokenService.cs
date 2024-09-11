using Api.Models;
using System.Security.Claims;

namespace Api.Service
{
    public interface ITokenService
    {         
        string CreateToken(UserAccount user);
        string CreateRefreshToken();
        ClaimsPrincipal ValidateToken(string token);
        Task<bool> ValidateRefreshToken(string token);
        Task SaveRefreshTokenAsync(string userId, string refreshToken);
        Task RemoveRefreshTokenAsync(string refreshToken);
    }
}
