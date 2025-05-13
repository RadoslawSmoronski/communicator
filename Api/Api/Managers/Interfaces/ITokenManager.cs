using Api.Models;
using Api.Models.Dtos.Controllers.UserController;
using Api.Models.Dtos.Service;
using Api.Utilities.Result;
using System.Security.Claims;

namespace Api.Managers.Interfaces
{
    public interface ITokenManager
    {
        //Access Token
        Task<ResultT<string>> CreateAccessTokenAsync(UserAccount? user);
        Task<ResultT<RefreshAccessTokenDto>> RefreshAccessTokenAsync(Guid refreshToken);

        //RefreshToken
        Task<ResultT<Guid>> CreateRefreshTokenAsync(Guid userId);
        Task<ResultT<int>> RemoveExpiredRefreshTokensAsync();
    }
}
