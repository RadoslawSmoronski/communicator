using Application.DTOs;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<LoggedUserDto>> LoginAsync(string email, string password);
        //Task<Guid?> RegisterAsync(string email, string password);
    }
}
