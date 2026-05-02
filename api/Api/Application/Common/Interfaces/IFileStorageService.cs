using Microsoft.AspNetCore.Http;
using Shared.Result;

namespace Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<Result> SaveFileAsync(IFormFile file, string directory, string filename);
        Result DeleteFile(string directory, string filename);
    }
}
