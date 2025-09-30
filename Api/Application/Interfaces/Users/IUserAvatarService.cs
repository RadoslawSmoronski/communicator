using Microsoft.AspNetCore.Http;
using Shared.Result;

namespace Application.Interfaces.Users
{
    public interface IUserAvatarService
    {
        Task<Result<string>> UploadAvatarAsync(Guid userId, IFormFile file);
        Task<Result> DeleteAvatarAsync(Guid userId);
        Task<Result<string>> ChangeAvatarAsync(Guid UserId, IFormFile file);
        string GetPublicAvatarUrl(string fileName);
    }
}
