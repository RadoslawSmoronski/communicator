using Api.Models;
using Api.Models.Dtos;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;

        public UserController(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            var hasher = new PasswordHasher<IdentityUser>();

            var user = new UserAccount()
            {
                UserName = registerDto.UserName,
                PasswordHash = hasher.HashPassword(null, registerDto.Password)
            };

            var result = await _userManager.CreateAsync(user);

            if(result.Succeeded)
            {
                var response = new RegisterResponseDto()
                {
                    Succeeded = true,
                    Message = "The user has been successfully created.",
                    User = registerDto
                };
                return Ok(response);
            }
            else
            {
                var response = new RegisterResponseDto()
                {
                    Succeeded = false,
                    Message = "Validation failed.",
                    User = registerDto
                };
                return BadRequest(response);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            var result = await _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false);

            if (result.Succeeded)
            {
                var response = new LoginResponseDto()
                {
                    Succeeded = true,
                    Message = "The user has been successfully logged in.",
                    User = loginDto
                };
                return Ok(response);
            }
            else
            {
                var response = new LoginResponseDto()
                {
                    Succeeded = false,
                    Message = "Validation failed.",
                    User = loginDto
                };
                return BadRequest(response);
            }
        }

    }
}
