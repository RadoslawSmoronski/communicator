using Application.Common.Interfaces;
using Application.DTOs;
using Application.Interfaces;
using Application.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Result;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace Infrastructure.Identity
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ILogger<UserService> _logger;
        private readonly IUser _user;
        private readonly UserAvatarSettings _userAvatarSettings;
        private readonly IFileStorageService _fileStorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, ILogger<UserService> logger, IUser user, IOptions<UserAvatarSettings> userAvatarSettingsOption, IFileStorageService fileStorageService, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _user = user;
            _userAvatarSettings = userAvatarSettingsOption.Value;
            _fileStorageService = fileStorageService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<LoggedUserDto>> LoginAsync(string email, string password)
        {
            _logger.LogInformation("[UserService - LoginAsync] Login attempt for email: {Email}", email);

            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user is null)
                {
                    _logger.LogWarning("[UserService - LoginAsync] User not found for email: {Email}", email);
                    return Error.NotFound("UserNotFound", $"User with email '{email}' was not found.");
                }

                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("[UserService - LoginAsync] Login succeeded for user: {UserId}", user.Id);

                    var avatarUrl = user.AvatarUrl != null ? GetPublicAvatarUrl(user.AvatarUrl) : null;

                    return new LoggedUserDto()
                    {
                        Id = user.Id,
                        UserName = user.UserName!,
                        AvatarUrl = avatarUrl
                    };
                }

                _logger.LogWarning("[UserService - LoginAsync] Invalid credentials for email: {Email}", email);
                return Error.Unauthorized("InvalidCredentials", "Invalid email or password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserService - LoginAsync] Unexpected error for email: {Email}", email);
                return Error.Failure("LoginFailed", "An unexpected error occurred during login.");
            }
        }

        public async Task<Result> ConfirmEmailAsync(Guid userId, string confirmationToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return Error.NotFound("", "");

            var result = await _userManager.ConfirmEmailAsync(user, confirmationToken);

            if (result.Succeeded)
            {
                return Result.Success();
            }

            return Error.Failure("", "");
        }

        public async Task<Result<PasswordResetToken>> GeneratePasswordResetTokenAsync(string email)
        {
            _logger.LogInformation("[UserService - GeneratePasswordResetTokenAsync] Password reset token request for email: {Email}", email);

            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    _logger.LogWarning("[UserService - GeneratePasswordResetTokenAsync] User not found for email: {Email}", email);
                    return Error.NotFound("UserNotFound", $"User with email '{email}' was not found.");
                }

                _logger.LogInformation("[UserService - GeneratePasswordResetTokenAsync] Password reset token generated for user: {UserId}", user.Id);
                
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                return new PasswordResetToken()
                {
                    UserId = user.Id,
                    Token = token
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserService - GeneratePasswordResetTokenAsync] Unexpected error for email: {Email}", email);
                return Error.Failure("PasswordResetTokenFailed", "An unexpected error occurred while generating the password reset token.");
            }
        }

        public async Task<Result<string>> ResetPasswordAsync(Guid userId, string token, string newPassword)
        {
            _logger.LogInformation("[UserService - ResetPasswordAsync] Password reset attempt for userId: {UserId}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user is null)
                {
                    _logger.LogWarning("[UserService - ResetPasswordAsync] User not found for userId: {UserId}", userId);
                    return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
                }

                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("[UserService - ResetPasswordAsync] Password reset succeeded for userId: {UserId}", userId);
                    return newPassword;
                }

                var errorDescription = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("[UserService - ResetPasswordAsync] Password reset failed for userId: {UserId}. Errors: {Errors}", userId, errorDescription);
                return Error.Failure("PasswordResetFailed", errorDescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserService - ResetPasswordAsync] Unexpected error for userId: {UserId}", userId);
                return Error.Failure("PasswordResetException", "An unexpected error occurred during password reset.");
            }
        }

        public async Task<Result<RegisteredDto>> RegisterAsync(string email, string username, string password)
        {
            _logger.LogInformation("[UserService - RegisterAsync] Registration attempt for email: {Email}, username: {Username}", email, username);

            try
            {
                var user = new UserAccount { Email = email, UserName = username };
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("[UserService - RegisterAsync] Registration succeeded for user: {UserId}", user.Id);
                    return new RegisteredDto()
                    {
                        Id = user.Id,
                        Email = email,
                        Username = username
                    };
                }

                var conflictEmailError = result.Errors.FirstOrDefault(e => e.Code == "DuplicateEmail");
                if (conflictEmailError != null)
                {
                    _logger.LogWarning("[UserService - RegisterAsync] Email conflict for email: {Email}", email);
                    return Error.Conflict("DuplicateEmail", $"Email '{email}' is already in use.");
                }

                var conflictUsernameError = result.Errors.FirstOrDefault(e => e.Code == "DuplicateUserName");
                if (conflictUsernameError != null)
                {
                    _logger.LogWarning("[UserService - RegisterAsync] Username conflict for username: {Username}", username);
                    return Error.Conflict("DuplicateUsername", $"Username '{username}' is already in use.");
                }

                var errorDescription = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("[UserService - RegisterAsync] Registration failed for email: {Email}, username: {Username}. Errors: {Errors}", email, username, errorDescription);
                return Error.Failure("RegistrationFailed", errorDescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserService - RegisterAsync] Unexpected error for email: {Email}, username: {Username}", email, username);
                return Error.Failure("RegistrationException", "An unexpected error occurred during registration.");
            }
        }

        public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(Guid userId)
        {
            _logger.LogInformation("[UserService - GenerateEmailConfirmationTokenAsync] Generating email confirmation token for userId: {UserId}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user is null)
                {
                    _logger.LogWarning("[UserService - GenerateEmailConfirmationTokenAsync] User not found for userId: {UserId}", userId);
                    return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                _logger.LogInformation("[UserService - GenerateEmailConfirmationTokenAsync] Token generated for userId: {UserId}", userId);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserService - GenerateEmailConfirmationTokenAsync] Unexpected error for userId: {UserId}", userId);
                return Error.Failure("EmailConfirmationTokenFailed", "An unexpected error occurred while generating the email confirmation token.");
            }
        }

        public bool IsAuthorized(Guid userId)
        {
            var currentUserId = _user.Id;
            var isAdmin = _user.Roles?.Contains("Admin") ?? false;

            if (userId == currentUserId || isAdmin)
            {
                return true;
            }

            return false;
        }

        public async Task<Result<string>> ChangeUsernameAsync(Guid userId, string newUsername)
        {
            _logger.LogInformation("[UserService - ChangeNameAsync] Change username attempt for userId: {UserId}, newUsername: {NewUsername}", userId, newUsername);

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user is null)
                {
                    _logger.LogWarning("[UserService - ChangeUsernameAsync] User not found for userId: {UserId}", userId);
                    return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
                }

                var isUsernameExists = await _userManager.FindByNameAsync(newUsername);

                if (isUsernameExists != null)
                {
                    _logger.LogWarning("[UserService - ChangeUsernameAsync] Username conflict for newUsername: {NewUsername}", newUsername);
                    return Error.Conflict("DuplicateUsername", $"Username '{newUsername}' is already in use.");
                }

                var result = await _userManager.SetUserNameAsync(user, newUsername);

                if (result.Succeeded)
                {
                    _logger.LogInformation("[UserService - ChangeUsernameAsync] Username changed successfully for userId: {UserId}", userId);
                    return newUsername;
                }

                var errorDescription = string.Join("; ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("[UserService - ChangeUsernameAsync] Username change failed for userId: {UserId}. Errors: {Errors}", userId, errorDescription);
                return Error.Failure("ChangeNameFailed", errorDescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserService - ChangeUsernameAsync] Unexpected error for userId: {UserId}", userId);
                return Error.Failure("ChangeNameException", "An unexpected error occurred during username change.");
            }
        }

        public async Task<Result> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            _logger.LogInformation("[UserService - ChangePasswordAsync] Password change attempt for userId: {UserId}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("[UserService - ChangePasswordAsync] User not found for userId: {UserId}", userId);
                    return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
                }

                var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("[UserService - ChangePasswordAsync] Password changed successfully for userId: {UserId}", userId);
                    return Result.Success();
                }
                else
                {
                    var passwordErrorCodes = new List<string>
                    {
                        "PasswordRequireDigit",
                        "PasswordRequireLower",
                        "PasswordRequireNonLetterOrDigit",
                        "PasswordRequireUpper",
                        "PasswordTooShort"
                    };

                    bool hasAnyPasswordError = result.Errors.Any(e => passwordErrorCodes.Contains(e.Code));
                    var errorDescription = string.Join("; ", result.Errors.Select(e => e.Description));

                    if (hasAnyPasswordError)
                    {
                        _logger.LogWarning("[UserService - ChangePasswordAsync] Password validation failed for userId: {UserId}. Errors: {Errors}", userId, errorDescription);
                        return Error.Validation("PasswordValidationFailed", errorDescription);
                    }

                    _logger.LogWarning("[UserService - ChangePasswordAsync] Password change unauthorized for userId: {UserId}. Errors: {Errors}", userId, errorDescription);
                    return Error.Unauthorized("ChangePasswordUnauthorized", errorDescription);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserService - ChangePasswordAsync] Unexpected error for userId: {UserId}", userId);
                return Error.Unknown("ChangePasswordException", "An unexpected error occurred during password change.");
            }
        }

        public async Task<Result<string>> UploadAvatarAsync(Guid userId, IFormFile file)
        {
            _logger.LogInformation("[UserService - UploadAvatarAsync] Upload avatar attempt for userId: {UserId}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("[UserService - UploadAvatarAsync] User not found for userId: {UserId}", userId);
                    return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
                }
                else if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    _logger.LogWarning("[UserService - UploadAvatarAsync] User already has an avatar. userId: {UserId}", userId);
                    return Error.Conflict("AvatarAlreadyExists", "User already has an avatar.");
                }

                var isAvatarFileValidAsync = await IsAvatarFileValidAsync(file);

                if (!isAvatarFileValidAsync.IsSuccess)
                {
                    _logger.LogWarning("[UserService - UploadAvatarAsync] Avatar file validation failed for userId: {UserId}. Error: {Error}", userId, isAvatarFileValidAsync.Error?.Description);
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
                        _logger.LogInformation("[UserService - UploadAvatarAsync] Avatar uploaded successfully for userId: {UserId}", userId);
                        return filename;
                    }

                    var errorDescription = string.Join("; ", result.Errors.Select(e => e.Description));
                    _logger.LogError("[UserService - UploadAvatarAsync] Failed to update user with new avatar for userId: {UserId}. Errors: {Errors}", userId, errorDescription);
                    return Error.Failure("AvatarUpdateFailed", errorDescription);
                }

                _logger.LogError("[UserService - UploadAvatarAsync] Failed to save avatar file for userId: {UserId}. Error: {Error}", userId, saveFileResult.Error?.Description);
                return Error.Failure("AvatarSaveFailed", saveFileResult.Error?.Description ?? "Failed to save avatar file.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserService - UploadAvatarAsync] Unexpected error for userId: {UserId}", userId);
                return Error.Unknown("UploadAvatarException", "An unexpected error occurred during avatar upload.");
            }
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

        public async Task<Result> DeleteAvatarAsync(Guid userId)
        {
            _logger.LogInformation("[UserService - DeleteAvatarAsync] Delete avatar attempt for userId: {UserId}", userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user is null)
                {
                    _logger.LogWarning("[UserService - DeleteAvatarAsync] User not found for userId: {UserId}", userId);
                    return Error.NotFound("UserNotFound", $"User with id '{userId}' was not found.");
                }

                if (string.IsNullOrEmpty(user.AvatarUrl))
                {
                    _logger.LogWarning("[UserService - DeleteAvatarAsync] No avatar to delete for userId: {UserId}", userId);
                    return Error.NotFound("AvatarNotFound", "User does not have an avatar to delete.");
                }

                var result = _fileStorageService.DeleteFile("avatars", user.AvatarUrl);

                if (result.IsSuccess)
                {
                    user.AvatarUrl = null;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (updateResult.Succeeded)
                    {
                        _logger.LogInformation("[UserService - DeleteAvatarAsync] Avatar deleted successfully for userId: {UserId}", userId);
                        return Result.Success();
                    }
                    else
                    {
                        var errorDescription = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                        _logger.LogError("[UserService - DeleteAvatarAsync] Failed to update user after avatar deletion for userId: {UserId}. Errors: {Errors}", userId, errorDescription);
                        return Error.Failure("AvatarDeleteUpdateFailed", errorDescription);
                    }
                }
                else
                {
                    _logger.LogError("[UserService - DeleteAvatarAsync] Failed to delete avatar file for userId: {UserId}. Error: {Error}", userId, result.Error?.Description);
                    return Error.Failure("AvatarDeleteFailed", result.Error?.Description ?? "Failed to delete avatar file.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserService - DeleteAvatarAsync] Unexpected error for userId: {UserId}", userId);
                return Error.Unknown("DeleteAvatarException", "An unexpected error occurred during avatar deletion.");
            }
        }

        private string GetPublicAvatarUrl(string fileName)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}/avatars/{fileName}";
        }

    }
}
