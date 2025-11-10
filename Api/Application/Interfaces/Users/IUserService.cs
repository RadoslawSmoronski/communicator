using Application.Auth.Commands.RequestPasswordReset;
using Domain.Entities;
using Shared.Result;

namespace Application.Interfaces.Users
{
    public interface IUserService
    {
        Task<Result<User>> LoginAsync(string email, string password);
        Task<Result> ConfirmEmailAsync(Guid userId, string confirmationToken);
        Task<Result<RequestPasswordResetReadModel>> GeneratePasswordResetTokenAsync(string email);
        Task<Result<string>> ResetPasswordAsync(Guid userId, string token, string newPassword);
        Task<Result<User>> RegisterAsync(string email, string username, string password);
        Task<Result<string>> GenerateEmailConfirmationTokenAsync(Guid userId);
        Task<Result> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
        Task<Result<string>> ChangeUsernameAsync(Guid userId, string newUsername);
        Task<Result<List<User>>> GetAllAsync();
    }
}