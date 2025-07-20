using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Http;

namespace ChatCommunicator.Application.Services.Interfaces
{
    public interface IUserAvatarService
    {
        Task<ResultT<string>> UploadAvatarAsync(IFormFile? file);

        Task<Result> DeleteAvatarAsync(string filename);
        string GetPublicAvatarUrl(string fileName);
    }
}
