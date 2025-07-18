using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Infrastructure.Services.Interfaces;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ChatCommunicator.Application.Services
{
    public class UserAvatarService : IUserAvatarService
    {
        private const int MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024; // 5MB
        private const int MAX_IMAGE_WIDTH = 500; // 500 pixels
        private const int MAX_IMAGE_HEIGHT = 500; // 500 pixels
        private const int MIN_IMAGE_WIDTH = 100; // 100 pixels
        private const int MIN_IMAGE_HEIGHT = 100; // 100 pixels
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

        private readonly IFileStorageService _fileStorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAvatarService(IFileStorageService fileStorageService, IHttpContextAccessor httpContextAccessor)
        {
            _fileStorageService = fileStorageService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResultT<string>> UploadAvatarAsync(IFormFile? file)
        {
            if (file is null)
            {
                return Error.Validation("FILE_IS_EMPTY", "File is empty.");
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return Error.Validation("INVALID_FORMAT", "File format is not supported.");
            }

            if (file.Length > MAX_FILE_SIZE_BYTES) 
            {
                return Error.Validation("FILE_IS_TOO_BIG", "File size exceeds the maximum limit of 2MB.");
            }

            using (var stream = file.OpenReadStream())
            {
                try
                {
                    using (var image = await Image.LoadAsync<Rgba32>(stream))
                    {
                        if (image.Width > MAX_IMAGE_WIDTH || image.Height > MAX_IMAGE_HEIGHT)
                        {
                            return Error.Validation("FILE_IS_TOO_LARGE", "Image resolution exceeds the maximum allowed 500x500 pixels.");
                        }
                        else if (image.Width < MIN_IMAGE_WIDTH || image.Height < MIN_IMAGE_HEIGHT)
                        {
                            return Error.Validation("FILE_IS_TOO_SMALL", "Image resolution is too small. Minimum allowed is 100x100 pixels.");
                        }
                    }
                }
                catch
                {
                    return Error.Validation("INVALID_IMAGE", "File is not a valid image.");
                }
            }

            var filename = "avatar_" + Guid.NewGuid().ToString() + extension;

            var saveFile = await _fileStorageService.SaveFileAsync(file, "avatars", filename);

            if (!saveFile.IsSuccess)
            {
                return Error.Failure("FILE_UPLOAD_FAILED", "Failed to upload the avatar file.");
            }

            return filename;
        }

        public string GetPublicAvatarUrl(string fileName)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}/avatars/{fileName}";
        }
    }
}
