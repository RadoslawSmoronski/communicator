using Application.Auth.Commands.RequestPasswordReset;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using Application.Settings;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Result;


namespace Infrastructure.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ILogger<UserService> _logger;
        private readonly ICurrentUser _user;
        private readonly UserAvatarSettings _userAvatarSettings;
        private readonly IUserAvatarService _userAvatarService;
        private readonly IMapper _mapper;

        public UserService(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, ILogger<UserService> logger, ICurrentUser user, IOptions<UserAvatarSettings> userAvatarSettingsOption, IUserAvatarService userAvatarService, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _user = user;
            _userAvatarSettings = userAvatarSettingsOption.Value;
            _userAvatarService = userAvatarService;
            _mapper = mapper;
        }

        public async Task<Result<User>> LoginAsync(string email, string password)
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

                    var avatarUrl = user.AvatarUrl != null ? _userAvatarService.GetPublicAvatarUrl(user.AvatarUrl) : null;

                    return _mapper.Map<User>(user);
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

        public async Task<Result<RequestPasswordResetReadModel>> GeneratePasswordResetTokenAsync(string email)
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

                return new RequestPasswordResetReadModel(
                    UserId: user.Id,
                    Token: token);
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

        public async Task<Result<User>> RegisterAsync(string email, string username, string password)
        {
            _logger.LogInformation("[UserService - RegisterAsync] Registration attempt for email: {Email}, username: {Username}", email, username);

            try
            {
                var user = new UserAccount { Email = email, UserName = username };
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("[UserService - RegisterAsync] Registration succeeded for user: {UserId}", user.Id);
                    return _mapper.Map<User>(user);
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
        public async Task<Result<List<User>>> GetAllAsync()
            => _mapper.Map<List<User>>(await _userManager.Users.ToListAsync());
    }
}
