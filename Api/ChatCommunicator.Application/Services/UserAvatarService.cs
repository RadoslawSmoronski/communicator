using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Http;

namespace ChatCommunicator.Application.Services
{
    public class UserAvatarService : IUserAvatarService
    {
        public async Task<ResultT<string>> UploadAvatarAsync(IFormFile file)
        {
            if (file.Length > 5 * 1024 * 1024) 
            {
                return Error.Validation("FILE_IS_TOO_BIG", "File size exceeds the maximum limit of 2MB.");
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            var filename = "avatar_" + Guid.NewGuid().ToString() + extension;
            return filename;
        }
    }
}
