using Api.Models;
using Api.Models.Dtos.Controllers.UserController;
using Api.Utilities.Result;
using System.Security.Claims;

namespace Api.Managers.Interfaces
{
    public interface ITokenManager
    {
        //Access Token
        Task<ResultT<string>> CreateAccessTokenAsync(UserAccount user);
        Task<ResultT<string>> RefreshAccessTokenAsync(string refreshToken);

        //RefreshToken
        Task<ResultT<string>> CreateRefreshTokenAsync(string userId);

    }
}
