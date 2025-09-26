using Application.DTOs;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<LoggedUserDto>> LoginAsync(string email, string password);
        Task<Result> ConfirmEmailAsync(Guid userId, string confirmationToken);
        Task<Result<PasswordResetToken>> GeneratePasswordResetTokenAsync(string email);
        //Task<Guid?> RegisterAsync(string email, string password);
    }
}