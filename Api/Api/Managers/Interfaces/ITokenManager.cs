using Api.Models;
using Api.Models.Dtos.Controllers.UserController;
using Api.Models.Results.Managers.TokenManager;
using System.Security.Claims;

namespace Api.Managers.Interfaces
{
    public interface ITokenManager
    {
        //Access Token
        Task<CreateAccessTokenResult> CreateAccessTokenAsync(UserAccount user);
        Task<ClaimsPrincipal> ValidateAccessTokenAsync(string token);
        Task<string> RefreshAccessTokenAsync(string refreshToken);

        //RefreshToken
        Task<string> CreateRefreshTokenAsync();
        Task<bool> ValidateRefreshTokenAsync(string token);
        Task SaveRefreshTokenAsync(string userId, string refreshToken);
        Task RemoveRefreshTokenAsync(string refreshToken);

    }
}
