using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Shared.Result;
using System.Net;

namespace Infrastructure.Identity
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
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
                    return new LoggedUserDto()
                    {
                        Id = user.Id,
                        UserName = user.UserName!,
                        AvatarUrl = user.AvatarUrl
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
    }
}
