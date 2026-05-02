using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using Application.Common.Settings;
using Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Result;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Infrastructure.Services.Users
{
    public class UserAvatarService(
        ILogger<UserAvatarService> logger,
        UserManager<UserAccount> userManager,
        IOptions<UserAvatarSettings> userAvatarSettings,
        IFileStorageService fileStorageService,
        IHttpContextAccessor httpContextAccessor)
        : IUserAvatarService
    {
        private readonly ILogger<UserAvatarService> _logger = logger;
        private readonly UserManager<UserAccount> _userManager = userManager;
        private readonly UserAvatarSettings _userAvatarSettings = userAvatarSettings.Value;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<Result<string>> UploadAvatarAsync(Guid userId, IFormFile file)
        {
            _logger.LogInformation("[UserAvatarService - UploadAvatarAsync] Upload avatar attempt for userId: {UserId}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("[UserAvatarService - UploadAvatarAsync] User not found for userId: {UserId}", userId);
                    return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
                }
                else if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    _logger.LogWarning("[UserAvatarService - UploadAvatarAsync] User already has an avatar. userId: {UserId}", userId);
                    return Error.Conflict("AvatarAlreadyExists", "User already has an avatar.");
                }
                
                return await UploadAvatarAsync(user, file, "UploadAvatarAsync");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserAvatarService - UploadAvatarAsync] Unexpected error for userId: {UserId}", userId);
                return Error.Unknown("UploadAvatarException", "An unexpected error occurred during avatar upload.");
            }
        }
        public async Task<Result> DeleteAvatarAsync(Guid userId)
        {
            _logger.LogInformation("[UserAvatarService - DeleteAvatarAsync] Delete avatar attempt for userId: {UserId}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user is null)
                {
                    _logger.LogWarning("[UserAvatarService - DeleteAvatarAsync] User not found for userId: {UserId}", userId);
                    return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
                }

                return await DeleteAvatarAsync(user, "DeleteAvatarAsync");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserAvatarService - DeleteAvatarAsync] Unexpected error for userId: {UserId}", userId);
                return Error.Unknown("DeleteAvatarException", "An unexpected error occurred during avatar deletion.");
            }
        }

        public async Task<Result<string>> ChangeAvatarAsync(Guid userId, IFormFile file)
        {
            _logger.LogInformation("[UserAvatarService - ChangeAvatarAsync] Change avatar attempt for userId: {UserId}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("[UserAvatarService - ChangeAvatarAsync] User not found for userId: {UserId}", userId);
                    return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
                }
                else if (string.IsNullOrEmpty(user.AvatarUrl))
                {
                    _logger.LogWarning("[UserAvatarService - ChangeAvatarAsync] User does not have an avatar. userId: {UserId}", userId);
                    return Error.Conflict("AvatarNotFound", "User does not have an avatar to change.");
                }

                var deleteAvatarResult = await DeleteAvatarAsync(user, "ChangeAvatarAsync");
                if (!deleteAvatarResult.IsSuccess)
                {
                    _logger.LogWarning("[UserAvatarService - ChangeAvatarAsync] Failed to delete existing avatar for userId: {UserId}. Error: {ErrorDescription}", userId, deleteAvatarResult.Error?.Description);
                    return deleteAvatarResult.Error!;
                }

                var uploadResult = await UploadAvatarAsync(user, file, "ChangeAvatarAsync");
                return uploadResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserAvatarService - ChangeAvatarAsync] Unexpected error for userId: {UserId}", userId);
                return Error.Unknown("ChangeAvatarException", "An unexpected error occurred during avatar change.");
            }
        }

        public string GetPublicAvatarUrl(string fileName)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}/avatars/{fileName}";
        }

        private async Task<Result<string>> UploadAvatarAsync(UserAccount user, IFormFile file, string loggerTag)
        {
            var isAvatarFileValidAsync = await IsAvatarFileValidAsync(file);

            if (!isAvatarFileValidAsync.IsSuccess)
            {
                _logger.LogWarning("[UserAvatarService - {LoggerTag}] Avatar file validation failed for userId: {UserId}. Error: {ErrorDescription}", loggerTag, user.Id, isAvatarFileValidAsync.Error?.Description);
                return isAvatarFileValidAsync.Error!;
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            var filename = "avatar_" + Guid.NewGuid().ToString() + extension;

            var saveFileResult = await _fileStorageService.SaveFileAsync(file, "avatars", filename);

            if (saveFileResult.IsSuccess)
            {
                user.AvatarUrl = filename;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("[UserAvatarService - {LoggerTag}] Avatar uploaded successfully for userId: {UserId}", loggerTag, user.Id);
                    return GetPublicAvatarUrl(filename);
                }

                var errorDescription = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogError("[UserAvatarService - {LoggerTag}] Failed to update user with new avatar for userId: {UserId}. Errors: {Errors}", loggerTag, user.Id, errorDescription);
                return Error.Failure("AvatarUpdateFailed", errorDescription);
            }

            _logger.LogError("[UserAvatarService - {LoggerTag}] Failed to save avatar file for userId: {UserId}. Error: {ErrorDescription}", loggerTag, user.Id, saveFileResult.Error?.Description);
            return Error.Failure("AvatarSaveFailed", saveFileResult.Error?.Description ?? "Failed to save avatar file.");
        }

        private async Task<Result> IsAvatarFileValidAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (!_userAvatarSettings.AllowedExtensions.Contains(extension))
            {
                _logger.LogWarning("[UserService - IsAvatarFileValidAsync] Invalid file extension: {Extension}", extension);
                return Error.Validation("InvalidAvatarExtension", $"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", _userAvatarSettings.AllowedExtensions)}.");
            }

            if (file.Length > _userAvatarSettings.MaxFileSizeBytes)
            {
                _logger.LogWarning("[UserService - IsAvatarFileValidAsync] File size too large: {FileSize} bytes", file.Length);
                return Error.Validation("AvatarFileTooLarge", $"File size exceeds the maximum allowed size of {_userAvatarSettings.MaxFileSizeBytes} bytes.");
            }

            using (var stream = file.OpenReadStream())
            {
                try
                {
                    using (var image = await Image.LoadAsync<Rgba32>(stream))
                    {
                        if (image.Width > _userAvatarSettings.MaxImageWidth || image.Height > _userAvatarSettings.MaxImageHeight)
                        {
                            _logger.LogWarning("[UserService - IsAvatarFileValidAsync] Image dimensions too large: {Width}x{Height}", image.Width, image.Height);
                            return Error.Validation("AvatarImageTooLarge", $"Image dimensions exceed the maximum allowed size of {_userAvatarSettings.MaxImageWidth}x{_userAvatarSettings.MaxImageHeight} pixels.");
                        }
                        else if (image.Width < _userAvatarSettings.MinImageWidth || image.Height < _userAvatarSettings.MinImageHeight)
                        {
                            _logger.LogWarning("[UserService - IsAvatarFileValidAsync] Image dimensions too small: {Width}x{Height}", image.Width, image.Height);
                            return Error.Validation("AvatarImageTooSmall", $"Image dimensions are below the minimum required size of {_userAvatarSettings.MinImageWidth}x{_userAvatarSettings.MinImageHeight} pixels.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[UserService - IsAvatarFileValidAsync] File is not a valid image.");
                    return Error.Validation("InvalidAvatarImage", "The uploaded file is not a valid image or is corrupted.");
                }
            }

            return Result.Success();
        }

        private async Task<Result> DeleteAvatarAsync(UserAccount user, string loggerTag)
        {

            if (string.IsNullOrEmpty(user.AvatarUrl))
            {
                _logger.LogWarning("[UserAvatarService - {LoggerTag}] No avatar to delete for userId: {UserId}", loggerTag, user.Id);
                return Error.NotFound("AvatarNotFound", "User does not have an avatar to delete.");
            }

            var result = _fileStorageService.DeleteFile("avatars", user.AvatarUrl);

            if (result.IsSuccess)
            {
                user.AvatarUrl = null;
                var updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    _logger.LogInformation("[UserAvatarService - {LoggerTag}] Avatar deleted successfully for userId: {UserId}", loggerTag, user.Id);
                    return Result.Success();
                }
                else
                {
                    var errorDescription = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    _logger.LogError("[UserAvatarService - {LoggerTag}] Failed to update user after avatar deletion for userId: {UserId}. Errors: {Errors}", loggerTag, user.Id, errorDescription);
                    return Error.Failure("AvatarDeleteUpdateFailed", errorDescription);
                }
            }
            else
            {
                _logger.LogError("[UserAvatarService - {LoggerTag}] Failed to delete avatar file for userId: {UserId}. Error: {Error}", loggerTag, user.Id, result.Error?.Description);
                return Error.Failure("AvatarDeleteFailed", result.Error?.Description ?? "Failed to delete avatar file.");
            }
        }


    }
}