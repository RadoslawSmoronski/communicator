using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Http;

namespace ChatCommunicator.Infrastructure.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<Result> SaveFileAsync(IFormFile file, string directory, string filename);
        Task<Result> DeleteFileAsync(string directory, string filename);
    }
}
