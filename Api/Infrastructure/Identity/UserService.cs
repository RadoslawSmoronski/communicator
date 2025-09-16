using Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Shared.Result;

namespace Infrastructure.Identity
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;

        public UserService(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<Result<Guid>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return Error.NotFound("UserNotFound", $"User with email '{email}' was not found.");

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

            if (result.Succeeded)
                return user.Id;

            return Error.Unauthorized("InvalidCredentials", "Invalid email or password.");
        }
    }
}
