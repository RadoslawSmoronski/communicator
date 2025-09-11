using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Shared.Result;

namespace ChatCommunicator.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ResultT<string>> SendPasswordResetEmailAsync(string email);
        Task<ResultT<string>> ResetPasswordAsync(Guid userId, string token, string newPassword);
    }
}
