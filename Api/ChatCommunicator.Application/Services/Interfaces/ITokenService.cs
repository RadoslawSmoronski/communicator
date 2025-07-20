using ChatCommunicator.Contracts.Dtos.Service;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Shared.Result;

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
