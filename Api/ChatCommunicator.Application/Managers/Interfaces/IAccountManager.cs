using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using ChatCommunicator.Contracts.Dtos.Service;
using ChatCommunicator.Shared.Result;

namespace ChatCommunicator.Application.Managers.Interfaces
{
    public interface IAccountManager
    {
        Task<ResultT<SimpleUserDto>> RegisterAsync(RegisterDto registerDto);
        Task<ResultT<LoggedUserDto>> LoginAsync(LoginDto loginDto);
        Task<ResultT<RefreshAccessTokenDto>> RefreshAccessTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<ResultT<string>> ChangeUsernameAsync(string newUsername);
    }
}
