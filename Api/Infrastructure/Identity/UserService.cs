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
    }
}
