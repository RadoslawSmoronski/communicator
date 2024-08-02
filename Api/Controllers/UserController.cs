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
        private UserManager<UserAccount> _userManager;

        public UserController(UserManager<UserAccount> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromQuery] RegisterDto registerDto)
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
                    Message = "The user has been successfully created.",
                    User = registerDto
                };
                return Ok(response);
            }
            else
            {
                var response = new RegisterResponseDto()
                {
                    Message = "null",
                    User = registerDto
                };
                return BadRequest(response);
            }
        }

    }
}
