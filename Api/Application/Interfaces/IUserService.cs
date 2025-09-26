using Application.DTOs;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<LoggedUserDto>> LoginAsync(string email, string password);
        Task<Result> ConfirmEmailAsync(Guid userId, string confirmationToken);
        Task<Result<PasswordResetToken>> GeneratePasswordResetTokenAsync(string email);
        Task<Result<string>> ResetPasswordAsync(Guid userId, string token, string newPassword);
        Task<Result<RegisteredDto>> RegisterAsync(string email, string username, string password);
        Task<Result<string>> GenerateEmailConfirmationTokenAsync(Guid userId);
    }
}