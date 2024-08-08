using Api.Models;
using Api.Models.Dtos;
using Api.Models.Dtos.Controllers.UserController;
using Api.Service;
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
        private readonly ITokenService _tokenService;

        public UserController(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager,
            IMapper mapper, ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            var hasher = new PasswordHasher<IdentityUser>();

            var user = new UserAccount()
            {
                UserName = registerDto.UserName,
                PasswordHash = hasher.HashPassword(new IdentityUser { UserName = registerDto.UserName }, registerDto.Password),
            };

            var result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                var response = new RegisterResponseDto()
                {
                    Succeeded = true,
                    Message = "The user has been successfully created.",
                    User = new RegisteredUserDto()
                    {
                        UserName = registerDto.UserName,
                        Token = _tokenService.CreateToken(user)
                    }
                };
                return Ok(response);
            }
            else
            {
                var message = result.Errors.FirstOrDefault()?.Description ?? "Message error";

                var response = new RegisterResponseDto()
                {
                    Succeeded = false,
                    Message = message,
                    User = new RegisteredUserDto()
                    {
                        UserName = registerDto.UserName
                    }
                };
                return BadRequest(response);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            var result = await _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false);

            var user = await _userManager.FindByNameAsync(loginDto.UserName);

            if (result.Succeeded && user != null)
            {
                var response = new LoginResponseDto()
                {
                    Succeeded = true,
                    Message = "The user has been successfully logged in.",
                    User = new LoggedUserDto()
                    {
                        UserName = loginDto.UserName,
                        Token = _tokenService.CreateToken(user)
                    }
                };
                return Ok(response);
            }
            else
            {
                var response = new LoginResponseDto()
                {
                    Succeeded = false,
                    Message = "Login or password are incorect.",
                    User = new LoggedUserDto()
                    {
                        UserName = loginDto.UserName
                    }
                };
                return BadRequest(response);
            }
        }

        [HttpGet("getUsers")]
        public async Task<IActionResult> GetUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            var getUserDtoList = new List<GetUserDto>();

            foreach (var user in users)
            {
                var getUserDto = _mapper.Map<GetUserDto>(user);
                getUserDtoList.Add(getUserDto);
            }

            return Ok(getUserDtoList);
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

        [HttpDelete("deleteUserById/{id}")]
        public async Task<IActionResult> DeleteUserByIdAsync([FromRoute] int id)
        {
            var _id = id.ToString();

            if (!string.IsNullOrEmpty(_id))
            {
                var user = await _userManager.FindByIdAsync(_id);
                return await DeleteUserAsync(user);
            }

            var response = new DeleteResponseDto()
            {
                Succeeded = false,
                Message = "Id not specified as int.",
                User = null
            };
            return BadRequest(response);
        }

        private async Task<IActionResult> DeleteUserAsync(UserAccount? user)
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
