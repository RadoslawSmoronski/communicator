using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Http;

namespace ChatCommunicator.Application.Managers.Interfaces
{
    public interface IAccountManager
    {
        Task<ResultT<SimpleUserDto>> RegisterAsync(RegisterDto registerDto);
        Task<ResultT<LoggedUserDto>> LoginAsync(LoginDto loginDto);
        Task<ResultT<string>> ChangeUsernameAsync(Guid userId, string newUsername);
        Task<Result> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
        Task<ResultT<string>> UploadAvatarAsync(Guid userId, IFormFile? file);
        Task<ResultT<string>> ChangeAvatarAsync(Guid userId, IFormFile? file);
        Task<Result> DeleteAvatarAsync(Guid userId);
    }
}
