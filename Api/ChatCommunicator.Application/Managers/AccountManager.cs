using AutoMapper;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using ChatCommunicator.Infrastructure.Models;
using ChatCommunicator.Infrastructure.Services.Interfaces;
using ChatCommunicator.Shared.Result;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net;

namespace ChatCommunicator.Application.Managers
{
    public class AccountManager : IAccountManager
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IUserAvatarService _userAvatarService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountManager> _logger;
        private readonly ConfirmEmailMessageSettings _confirmEmailMessageSettings;

        public AccountManager(UserManager<UserAccount> userManager,
            IMapper mapper,
            SignInManager<UserAccount> signInManager,
            ITokenService tokenService,
            IEmailService emailService,
            IUserAvatarService userAvatarService,
            ILogger<AccountManager> logger,
            IOptions<ConfirmEmailMessageSettings> confirmEmailMessageSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _tokenService = tokenService;
            _userAvatarService = userAvatarService;
            _logger = logger;
            _emailService = emailService;
            _confirmEmailMessageSettings = confirmEmailMessageSettings.Value;
        }

        public async Task<ResultT<RegisteredDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with email: {email}", registerDto.Email);

                var user = new UserAccount { Email = registerDto.Email, UserName = registerDto.Username };
                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedToken = WebUtility.UrlEncode(token);

                    var emailContent = CreateEmailContent(user.Id, encodedToken);

                    await _emailService.SendAsync(user.Email, _confirmEmailMessageSettings.Title, emailContent);

                    var dto = _mapper.Map<RegisteredDto>(user);

                    dto.ConfirmToken = encodedToken;

                    return dto;
                }

                var conflictEmailError = result.Errors.FirstOrDefault(e => e.Code == "DuplicateEmail");
                if (conflictEmailError != null)
                {
                    _logger.LogWarning("Registration conflict: email already exists - {Email}", registerDto.Email);
                    return Error.Conflict("CONFLICT", "A user with this email already exists.");
                }

                var conflictUsernameError = result.Errors.FirstOrDefault(e => e.Code == "DuplicateUsername");
                if (conflictUsernameError != null)
                {
                    _logger.LogWarning("Registration conflict: username already exists - {Username}", registerDto.Username);
                    return Error.Conflict("CONFLICT", "A user with this username already exists.");
                }

                _logger.LogError("User registration failed unexpectedly for email: {Email}", registerDto.Email);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "User registration failed unexpectedly. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during registration of user: {Email}", registerDto.Email);
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }
        }

        public async Task<ResultT<LoggedUserDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Attempting login for Email: {Email}", loginDto.Email);
                var user = await _userManager.FindByEmailAsync(loginDto.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login failed: user not found - {Email}", loginDto.Email);
                    return Error.Unauthorized("UNAUTHORIZED", "Email or password is incorrect.");
                }
                ;

                var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);

                    var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id);
                    var accessToken = await _tokenService.CreateAccessTokenAsync(user);

                    if (refreshToken.IsSuccess && accessToken.IsSuccess)
                    {
                        var avatarUrl = user.AvatarUrl != null ? _userAvatarService.GetPublicAvatarUrl(user.AvatarUrl) : null;

                        var resultObj = new LoggedUserDto()
                        {
                            UserName = user.UserName!,
                            Id = user.Id,
                            AvatarUrl = avatarUrl,
                            AccessToken = accessToken.Value,
                            RefreshToken = refreshToken.Value
                        };

                        return resultObj;
                    }
                }

                _logger.LogWarning("Login failed for email: {Email}", loginDto.Email);
                return Error.Unauthorized("INVALID_CREDENTIALS", "Email or password is incorrect.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during login of user: {UserName}", loginDto.Email);
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }
        }

        public async Task<Result> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
        {
            var user = await _userManager.FindByIdAsync(confirmEmailDto.UserId.ToString());

            if (user == null)
                return Error.NotFound("test", "test"); //refactor

            var result = await _userManager.ConfirmEmailAsync(user, WebUtility.UrlDecode(confirmEmailDto.Token));

            if (result.Succeeded)
            {
                return Result.Success();
            }

            return Error.Failure("test", "test");
        }

        public async Task<ResultT<string>> ChangeUsernameAsync(Guid userId, string newUsername)
        {
            _logger.LogInformation("User {UserId} requested username change to: {NewUsername}", userId, newUsername);

            if (string.IsNullOrWhiteSpace(newUsername))
            {
                _logger.LogWarning("Username change failed: new username is empty or null");
                return Error.Validation("VALIDATION", "Username cannot be empty or null.");
            }
            ;

            if (Guid.Empty == userId)
            {
                _logger.LogWarning("Username change failed: userId is empty or null");
                return Error.Validation("VALIDATION_USERID", "UserId cannot be empty or null.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("Username change failed: user not found - {UserId}", userId);
                    return Error.Unauthorized("UNAUTHORIZED", "The user associated with the access token does not exist. Please log in again.");
                }
                ;

                var isUsernameExists = await _userManager.FindByNameAsync(newUsername);

                if (isUsernameExists != null)
                {
                    _logger.LogWarning("Username change conflict: username already taken - {NewUsername}", newUsername);
                    return Error.Conflict("CONFLICT", "The chosen username is already taken. Please choose a different one.");
                }

                var result = await _userManager.SetUserNameAsync(user, newUsername);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Username changed successfully for user {UserId} to {NewUsername}", userId, newUsername);
                    return newUsername;
                }

                _logger.LogError("Username change failed unexpectedly for user {UserId}", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "User registration failed unexpectedly. Please try again later or contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during username change for user {UserId}", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }
        }

        public async Task<Result> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            if (Guid.Empty == userId)
            {
                _logger.LogWarning("ChangePasswordAsync failed: userId is empty or null.");
                return Error.Validation("VALIDATION_USERID", "UserId cannot be empty or null.");
            }
            else if (string.IsNullOrEmpty(oldPassword))
            {
                _logger.LogWarning("ChangePasswordAsync failed: old password is empty or null.");
                return Error.Validation("VALIDATION_OLDPASSWORD", "Old password cannot be empty or null.");
            }
            else if (string.IsNullOrEmpty(newPassword))
            {
                _logger.LogWarning("ChangePasswordAsync failed: new password is empty or null.");
                return Error.Validation("VALIDATION_NEWPASSWORD", "New password cannot be empty or null.");
            }
            else if (oldPassword == newPassword)
            {
                _logger.LogWarning("ChangePasswordAsync failed: old password and new password are the same.");
                return Error.Validation("VALIDATION_PASSWORDS_SAME", "New password cannot be the same as the old password.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("ChangePasswordAsync failed: user with ID {UserId} not found.", userId);
                    return Error.Unauthorized("USER_NOT_FOUND", "User associated with the ID does not exist. Please log in again.");
                }

                var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("ChangePasswordAsync succeeded: password changed successfully for user {UserId}.", userId);
                    return Result.Success();
                }
                else
                {
                    var passwordErrorCodes = new List<string> { "PasswordRequireDigit", "PasswordRequireLower",
                        "PasswordRequireNonLetterOrDigit", "PasswordRequireUpper", "PasswordTooShort" };

                    bool hasAnyPasswordError = result.Errors.Any(e => passwordErrorCodes.Contains(e.Code));
                    if (hasAnyPasswordError)
                    {
                        _logger.LogWarning("ChangePasswordAsync failed: new password does not meet the required criteria for user {UserId}.", userId);
                        return Error.Validation("NEWPASSWORD_IS_NOT_VALID", "The new password does not meet the required criteria.");
                    }

                    _logger.LogWarning("ChangePasswordAsync failed: old password is incorrect for user {UserId}.", userId);
                    return Error.Unauthorized("OLDPASSWORD_IS_INCORRECT", "The old password is incorrect.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangePasswordAsync failed: exception occurred for user {UserId}.", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", "An unexpected error occurred. Please try again later.");
            }
        }

        public async Task<ResultT<string>> UploadAvatarAsync(Guid userId, IFormFile? file)
        {
            if (Guid.Empty == userId)
            {
                _logger.LogWarning("UploadAvatarAsync failed: userId is empty or null.");
                return Error.Validation("VALIDATION_USERID", "UserId cannot be empty or null.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("UploadAvatarAsync failed: user with ID {UserId} not found.", userId);
                    return Error.Unauthorized("USER_NOT_FOUND", "User associated with the id does not exist. Please log in again.");
                }
                else if (String.IsNullOrEmpty(user.AvatarUrl) == false)
                {
                    _logger.LogWarning("User {UserId} attempted to set avatar via POST but avatar is already set: {AvatarUrl}", userId, user.AvatarUrl);
                    return Error.Conflict("AVATAR_ALREADY_SET", "Avatar is already set.");
                }

                var uploadFileResult = await _userAvatarService.UploadAvatarAsync(file);

                if (uploadFileResult.IsSuccess)
                {
                    user.AvatarUrl = uploadFileResult.Value;

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User {UserId} avatar updated successfully.", userId);
                        return uploadFileResult.Value;
                    }

                    _logger.LogError("Failed to update avatar URL for user {UserId}. Errors: {Errors}", userId, string.Join(", ", result.Errors));
                    return Error.Unknown("USER_UPDATE_FAILED", "Failed to update user avatar URL in database.");
                }
                else if (uploadFileResult.Error != null)
                {
                    _logger.LogError("Avatar upload failed for user {UserId}. Error: {ErrorCode} - {ErrorMessage}",
                        userId, uploadFileResult.Error.Code, uploadFileResult.Error.Description);
                    return uploadFileResult.Error;
                }

                _logger.LogError("UploadAvatarAsync ended with unknown error for user {UserId}.", userId);
                return Error.Unknown("AVATAR_UPLOAD_FAILED", "Failed to upload avatar file.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during avatar upload for user {UserId}", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }

        }

        public async Task<ResultT<string>> ChangeAvatarAsync(Guid userId, IFormFile? file)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("ChangeAvatarAsync failed: userId is empty or null.");
                return Error.Validation("VALIDATION_USERID", "UserId cannot be empty or null.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("ChangeAvatarAsync failed: user with ID {UserId} not found.", userId);
                    return Error.Unauthorized("USER_NOT_FOUND", "User associated with the id does not exist. Please log in again.");
                }
                else if (string.IsNullOrEmpty(user.AvatarUrl))
                {
                    _logger.LogWarning("ChangeAvatarAsync failed: user {UserId} does not have an avatar set.", userId);
                    return Error.Conflict("AVATAR_NOT_SET", "User does not have an avatar set.");
                }

                var uploadFileResult = await _userAvatarService.UploadAvatarAsync(file);

                if (uploadFileResult.IsSuccess)
                {
                    var deleteAvatarResult = _userAvatarService.DeleteAvatar(user.AvatarUrl);

                    if (deleteAvatarResult.IsSuccess)
                    {
                        user.AvatarUrl = uploadFileResult.Value;

                        var result = await _userManager.UpdateAsync(user);

                        if (result.Succeeded)
                        {
                            _logger.LogInformation("ChangeAvatarAsync succeeded: avatar for user {UserId} was successfully updated.", userId);
                            return user.AvatarUrl;
                        }

                        _logger.LogError("ChangeAvatarAsync failed: unable to update user {UserId} after avatar upload. Errors: {Errors}", userId, string.Join(", ", result.Errors));
                        return Error.Unknown("USER_UPDATE_FAILED", "Failed to update user avatar URL in database.");
                    }

                    _logger.LogError("ChangeAvatarAsync failed: unable to delete existing avatar for user {UserId}.", userId);
                    return Error.Unknown("AVATAR_DELETE_FAILED", "Failed to delete avatar file.");
                }
                else if (uploadFileResult.Error != null)
                {
                    _logger.LogError("Avatar upload failed for user {UserId}. Error: {ErrorCode} - {ErrorMessage}",
                        userId, uploadFileResult.Error.Code, uploadFileResult.Error.Description);
                    return uploadFileResult.Error;
                }

                _logger.LogError("ChangeAvatarAsync failed: unable to upload new avatar for user {UserId}.", userId);
                return Error.Unknown("AVATAR_UPLOAD_FAILED", "Failed to upload avatar file.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChangeAvatarAsync failed: exception occurred for user {UserId}.", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }
        }

        public async Task<Result> DeleteAvatarAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                _logger.LogWarning("DeleteAvatarAsync failed: userId is empty or null.");
                return Error.Validation("VALIDATION_USERID", "UserId cannot be empty or null.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    _logger.LogWarning("DeleteAvatarAsync failed: user with ID {UserId} not found.", userId);
                    return Error.Unauthorized("USER_NOT_FOUND", "User associated with the id does not exist. Please log in again.");
                }
                else if (string.IsNullOrEmpty(user.AvatarUrl))
                {
                    _logger.LogWarning("DeleteAvatarAsync failed: user {UserId} does not have an avatar set.", userId);
                    return Error.Conflict("AVATAR_NOT_SET", "User does not have an avatar set.");
                }

                var deleteAvatarResult = _userAvatarService.DeleteAvatar(user.AvatarUrl);

                if (deleteAvatarResult.IsSuccess)
                {
                    user.AvatarUrl = null;

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("DeleteAvatarAsync succeeded: avatar for user {UserId} was successfully removed.", userId);
                        return Result.Success();
                    }

                    _logger.LogError("DeleteAvatarAsync failed: unable to update user {UserId} after avatar deletion. Errors: {Errors}", userId, string.Join(", ", result.Errors));
                    return Error.Unknown("USER_UPDATE_FAILED", "Failed to update user avatar URL in database.");
                }

                _logger.LogError("DeleteAvatarAsync failed: unable to delete avatar for user {UserId}.", userId);
                return Error.Unknown("AVATAR_DELETE_FAILED", "Failed to delete avatar file.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAvatarAsync failed: exception occurred for user {UserId}.", userId);
                return Error.Unknown("INTERNAL_SERVER_ERROR", ex.Message);
            }
        }

        private string CreateEmailContent(Guid userId, string token)
        {
            var address = $"{_confirmEmailMessageSettings.Address}/userId={userId.ToString()}?token={token}";
            return _confirmEmailMessageSettings.Content.Replace("[address]", address);
        }

    }
}