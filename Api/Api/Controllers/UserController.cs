using Api.Models;
using Api.Models.Dtos;
using Api.Models.Dtos.Controllers.UserController;
using AutoMapper;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly IMapper _mapper;
        private UserManager<UserAccount> userManager;
        private SignInManager<UserAccount> signInManager;

        public UserController(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        public UserController(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
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

        [HttpGet("getUsers")]
        public async Task<IActionResult> GetUsersAsync()
        {
            var users = _userManager.Users.ToList();
            return Ok(users);
        }

        [HttpGet("findUserByUserName/{username}")]
        public async Task<IActionResult> FindUserByUserNameAsync([FromRoute] string username)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == username);


            if (user != null)
            {
                var findUserDto = _mapper.Map<FindUserDto>(user);
                return Ok(findUserDto);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("findUserById/{id}")]
        public async Task<IActionResult> FindUserByIdAsync([FromRoute] int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);


            if (user != null)
            {
                var findUserDto = _mapper.Map<FindUserDto>(user);
                return Ok(findUserDto);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("deleteUserByUserName/{username}")]
        public async Task<IActionResult> DeleteUserByUserNameAsync([FromRoute] string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return await DeleteUserAsync(user);
        }

        public async Task<IActionResult> DeleteUserAsync(UserAccount? user)
        {
            if (user == null)
            {
                var response = new DeleteResponseDto()
                {
                    Succeeded = false,
                    Message = "Not found user.",
                    User = null
                };
                return BadRequest(response);
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    var response = new DeleteResponseDto()
                    {
                        Succeeded = true,
                        Message = "Deleted user.",
                        User = _mapper.Map<DeleteDto>(user)
                    };
                    return Ok(response);
                }
                else
                {
                    var response = new DeleteResponseDto()
                    {
                        Succeeded = false,
                        Message = "Not found user.",
                        User = _mapper.Map<DeleteDto>(user)
                    };
                    return BadRequest(response);
                }
            }
           
        }

    }
}
