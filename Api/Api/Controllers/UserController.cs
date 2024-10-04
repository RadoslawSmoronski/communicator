using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos.Controllers.UserController.LoginAsync;
using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
using Api.Models.Dtos.Responses;
using Api.Models.Dtos.Service;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly IMapper _mapper;
        private readonly ITokenManager _tokenManager;

        public UserController(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager,
            IMapper mapper, ITokenManager tokenManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _tokenManager = tokenManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            try
            {
                var user = new UserAccount { UserName = registerDto.UserName };
                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {

                    return Ok(new SuccessResponseWithResultDataDto<RegisteredUserDto>
                    {
                        Title = "User has been successfully created.",
                        ResultData = new Dictionary<string, RegisteredUserDto>
                        {
                            { "user", new RegisteredUserDto()
                                {
                                    UserName = user.UserName
                                }
                            }
                        },
                        TraceId = Activity.Current?.Id
                    });
                }

                var conflictError = result.Errors.FirstOrDefault(e => e.Code == "DuplicateUserName");
                if (conflictError != null)
                {
                    return Conflict(new Error409ResponseDto
                    {
                        Title = "User with this username already exists.",
                        TraceId = Activity.Current?.Id
                    });
                }

                return BadRequest(new Error400ResponseDto
                {
                    Title = "Invalid register attempt.",
                    TraceId = Activity.Current?.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Error500ResponseDto
                {
                    Title = "An internal server error occurred.",
                    TraceId = Activity.Current?.Id
                });
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByNameAsync(loginDto.UserName);
                if (user == null)
                {
                    return NotFound(new LoginFailedResponseDto
                    {
                        Succeeded = false,
                        Message = "User does not exist."
                    });
                }

                var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

                if (result.Succeeded)
                {
                    var refreshToken = await _tokenManager.CreateRefreshTokenAsync();
                    await _tokenManager.SaveRefreshTokenAsync(user.Id, refreshToken);

                    var accessToken = await _tokenManager.CreateAccessTokenAsync(user);

                    if(accessToken.Succeeded)
                    {
                        return Ok(new LoginResponseDto()
                        {
                            Succeeded = true,
                            Message = "The user has been successfully logged in.",
                            User = new LoggedUserDto()
                            {
                                UserName = loginDto.UserName,
                                AccessToken = accessToken.Token,
                                RefreshToken = refreshToken
                            }
                        });
                    }


                }

                return BadRequest(new LoginFailedResponseDto
                {
                    Succeeded = false,
                    Message = "Invalid login attempt."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginFailedResponseDto
                {
                    Succeeded = false,
                    Message = "An internal server error occurred."
                });
            }
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
                var newToken = await _tokenManager.RefreshAccessTokenAsync(refreshToken);

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
    }
}
