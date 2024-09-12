using Api.Models;
using Api.Models.Dtos.Controllers.UserController;
using System.Security.Claims;

namespace Api.Service
{
    public interface ITokenService
    {         
        //Access Token
        string CreateAccessToken(UserAccount user);
        ClaimsPrincipal ValidateAccessToken(string token);
        Task<RefreshAccessTokenDto> RefreshAccessToken(string refreshToken, string accessToken);

        //RefreshToken
        string CreateRefreshToken();
        Task<bool> ValidateRefreshToken(string token);
        Task SaveRefreshTokenAsync(string userId, string refreshToken);
        Task RemoveRefreshTokenAsync(string refreshToken);
    }
}
