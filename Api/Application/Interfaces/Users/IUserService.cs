using Application.DTOs;
using Shared.Result;

namespace Application.Interfaces.Users
{
    public interface IUserService
    {
        Task<Result<LoggedUserDto>> LoginAsync(string email, string password);
        bool IsAuthorized(Guid userId);
        Task<Result> ConfirmEmailAsync(Guid userId, string confirmationToken);
        Task<Result<PasswordResetToken>> GeneratePasswordResetTokenAsync(string email);
        Task<Result<string>> ResetPasswordAsync(Guid userId, string token, string newPassword);
        Task<Result<RegisteredDto>> RegisterAsync(string email, string username, string password);
        Task<Result<string>> GenerateEmailConfirmationTokenAsync(Guid userId);
        Task<Result> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
        Task<Result<string>> ChangeUsernameAsync(Guid userId, string newUsername);
    }
}