using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController;
using ChatCommunicator.Contracts.Dtos.Service;
using ChatCommunicator.Shared.Result;
using System.Security.Claims;

namespace ChatCommunicator.Application.Services.Interfaces
{
    public interface ITokenService
    {
        //Access Token
        Task<ResultT<string>> CreateAccessTokenAsync(UserAccount? user);
        Task<ResultT<RefreshAccessTokenDto>> RefreshAccessTokenAsync(Guid refreshToken);

        //RefreshToken
        Task<ResultT<Guid>> CreateRefreshTokenAsync(Guid userId);
    }
}
