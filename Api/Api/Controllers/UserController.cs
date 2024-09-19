using Api.Models;
using Api.Models.Dtos;
using Api.Models.Dtos.Controllers.UserController;
using Api.Models.Dtos.Controllers.UserController.LoginAsync;
using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
using Api.Service;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new UserAccount { UserName = registerDto.UserName };
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                var refreshToken = _tokenService.CreateRefreshToken();
                await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

                return Ok(new RegisterOkResponseDto
                {
                    Succeeded = true,
                    Message = "The user has been successfully created.",
                    User = new RegisteredUserDto
                    {
                        UserName = user.UserName,
                        RefreshToken = refreshToken,
                        AccessToken = _tokenService.CreateAccessToken(user)
                    }
                });
            }

            var conflictError = result.Errors.FirstOrDefault(e => e.Code == "DuplicateUserName");
            if (conflictError != null)
            {
                return Conflict(new RegisterFailedResponseDto
                {
                    Succeeded = false,
                    Errors = new List<string> { "User with this username already exists." }
                });
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            return StatusCode(500, new RegisterFailedResponseDto
            {
                Succeeded = false,
                Errors = errors
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok("test");
            //var result = await _signInManager.PasswordSignInAsync(loginDto.UserName, loginDto.Password, false, false);

            //var user = await _userManager.FindByNameAsync(loginDto.UserName);

            //if (result.Succeeded && user != null)
            //{
            //    var refreshToken = _tokenService.CreateRefreshToken();

            //    var response = new LoginResponseDto()
            //    {
            //        Succeeded = true,
            //        Message = "The user has been successfully logged in.",
            //        User = new LoggedUserDto()
            //        {
            //            UserName = loginDto.UserName,
            //            RefreshToken = refreshToken,
            //            AccessToken = _tokenService.CreateAccessToken(user),
            //        }
            //    };

            //    await _tokenService.SaveRefreshTokenAsync(user.Id, refreshToken);

            //    return Ok(response);
            //}
            //else
            //{
            //    var response = new LoginResponseDto()
            //    {
            //        Succeeded = false,
            //        Message = "Login or password are incorrect.",
            //        User = new LoggedUserDto()
            //        {
            //            UserName = loginDto.UserName
            //        }
            //    };
            //    return BadRequest(response);
            //}
        }

        [HttpPost("refreshAccessToken")]
        public async Task<IActionResult> refreshAccessToken([FromBody] string refreshToken)
        {

            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new RefreshAccessTokenResponseDto
                {
                    Succeeded = false,
                    Message = "Refresh token must not be null or empty."
                });
            }

            try
            {
                var newToken = await _tokenService.RefreshAccessToken(refreshToken);

                    var response = new RefreshAccessTokenResponseDto()
                    {
                        Succeeded = true,
                        Message = "The access token have been successfully refreshed.",
                        AccessToken = newToken
                    };

                    return Ok(response);

                }
            catch (Exception ex)
            {
                var response = new RefreshAccessTokenResponseDto()
                {
                    Succeeded = false,
                    Message = ex.Message
                };


                switch (ex)
                {
                    case ArgumentNullException argNullEx:
                        return BadRequest(response);

                    case UnauthorizedAccessException unauthorizedEx:
                        return Unauthorized(response);
                }

                return BadRequest(response);
            }

        }

        [HttpGet("searchTest")]
        [Authorize]
        public async Task<IActionResult> searchTestAsync([FromQuery] string input)
        {
            var response = new RefreshAccessTokenResponseDto()
            {
                Succeeded = true,
                Message = $"Received text: {input}"
            };

            return Ok(response);
        }

        //[HttpGet("getUsers")]
        //public async Task<IActionResult> GetUsersAsync()
        //{
        //    var users = await _userManager.Users.ToListAsync();

        //    var getUserDtoList = new List<GetUserDto>();

        //    foreach (var user in users)
        //    {
        //        var getUserDto = _mapper.Map<GetUserDto>(user);
        //        getUserDtoList.Add(getUserDto);
        //    }

        //    return Ok(getUserDtoList);
        //}

        //[HttpGet("findUserByUserName/{username}")]
        //public async Task<IActionResult> FindUserByUserNameAsync([FromRoute] string username)
        //{
        //    var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == username);


        //    if (user != null)
        //    {
        //        var findUserDto = _mapper.Map<FindUserDto>(user);
        //        return Ok(findUserDto);
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}

        //[HttpGet("findUserById/{id}")]
        //public async Task<IActionResult> FindUserByIdAsync([FromRoute] string id)
        //{
        //    var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);


        //    if (user != null)
        //    {
        //        var findUserDto = _mapper.Map<FindUserDto>(user);
        //        return Ok(findUserDto);
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}

        //[HttpDelete("deleteUserByUserName/{username}")]
        //public async Task<IActionResult> DeleteUserByUserNameAsync([FromRoute] string username)
        //{
        //    var user = await _userManager.FindByNameAsync(username);
        //    return await DeleteUserAsync(user);
        //}

        //[HttpDelete("deleteUserById/{id}")]
        //public async Task<IActionResult> DeleteUserByIdAsync([FromRoute] int id)
        //{
        //    var _id = id.ToString();

        //    if (!string.IsNullOrEmpty(_id))
        //    {
        //        var user = await _userManager.FindByIdAsync(_id);
        //        return await DeleteUserAsync(user);
        //    }

        //    var response = new DeleteResponseDto()
        //    {
        //        Succeeded = false,
        //        Message = "Id not specified as int.",
        //        User = null
        //    };
        //    return BadRequest(response);
        //}

        //private async Task<IActionResult> DeleteUserAsync(UserAccount? user)
        //{
        //    if (user == null)
        //    {
        //        var response = new DeleteResponseDto()
        //        {
        //            Succeeded = false,
        //            Message = "Not found user.",
        //            User = null
        //        };
        //        return BadRequest(response);
        //    }
        //    else
        //    {
        //        var result = await _userManager.DeleteAsync(user);

        //        if (result.Succeeded)
        //        {
        //            var response = new DeleteResponseDto()
        //            {
        //                Succeeded = true,
        //                Message = "Deleted user.",
        //                User = _mapper.Map<DeleteDto>(user)
        //            };
        //            return Ok(response);
        //        }
        //        else
        //        {
        //            var response = new DeleteResponseDto()
        //            {
        //                Succeeded = false,
        //                Message = "Not found user.",
        //                User = _mapper.Map<DeleteDto>(user)
        //            };
        //            return BadRequest(response);
        //        }
        //    }

        //}

    }
}
