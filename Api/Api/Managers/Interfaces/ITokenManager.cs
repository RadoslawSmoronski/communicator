using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController;
using ChatCommunicator.Contracts.Dtos.Service;
using ChatCommunicator.Shared.Result;
using System.Security.Claims;

namespace ChatCommunicator.Managers.Interfaces
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
