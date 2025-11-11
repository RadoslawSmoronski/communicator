using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Result;

namespace Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(ILogger<FileStorageService> logger)
        {
            _logger = logger;
        }

        public async Task<Result> SaveFileAsync(IFormFile file, string directory, string filename)
        {
            try
            {
                var folderPath = Path.Combine(_basePath, directory);
                _logger.LogInformation("Saving file to {FolderPath} with name {Filename}", folderPath, filename);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    _logger.LogInformation("Created directory {FolderPath}", folderPath);
                }

                var filePath = Path.Combine(folderPath, filename);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File saved successfully at {FilePath}", filePath);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the file");
                return Error.Failure("FILE_SAVE_ERROR", "An error occurred while saving the file.");
            }
        }

        public Result DeleteFile(string directory, string filename)
        {
            try
            {
                string fullPath = Path.Combine(_basePath, directory, filename);
                _logger.LogInformation("Deleting file at {FullPath}", fullPath);

                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("File not found at {FullPath}", fullPath);
                    return Error.Validation("FILE_NOT_FOUND", "File not found.");
                }

                File.Delete(fullPath);

                _logger.LogInformation("File deleted successfully at {FullPath}", fullPath);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file at {Directory}/{Filename}", directory, filename);
                return Error.Failure("DELETE_FAILED", $"Failed to delete file: {ex.Message}");
            }
        }
    }
}
