using Api.Models;
using Api.Models.Dtos.Controllers.UserController;
using System.Security.Claims;

namespace Api.Service
{
    public interface ITokenService
    {         
        //Access Token
        Task<string> CreateAccessTokenAsync(UserAccount user);
        Task<ClaimsPrincipal> ValidateAccessTokenAsync(string token);
        Task<string> RefreshAccessTokenAsync(string refreshToken);

        //RefreshToken
        Task<string> CreateRefreshTokenAsync();
        Task<bool> ValidateRefreshTokenAsync(string token);
        Task SaveRefreshTokenAsync(string userId, string refreshToken);
        Task RemoveRefreshTokenAsync(string refreshToken);

    }
}
