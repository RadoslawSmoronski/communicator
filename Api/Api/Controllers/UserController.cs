using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos.Controllers.UserController.LoginAsync;
using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
using Api.Models.Dtos.Responses;
using Api.Models.Dtos.Responses.Interfaces;
using Api.Models.Dtos.Service;
using Api.Utilities.Result;
using AutoMapper;
using Microsoft.AspNetCore.Hosting.Server;
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
        private readonly ResponseHttpFactory _responseFactory;

        public UserController(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager,
            IMapper mapper, ITokenManager tokenManager, ResponseHttpFactory responseFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _tokenManager = tokenManager;
            _responseFactory = responseFactory;
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
                    var resultData = new Dictionary<string, RegisteredUserDto>
                        {
                            { "user", new RegisteredUserDto()
                                {
                                    UserName = user.UserName
                                }
                            }
                        };

                    var response = _responseFactory.Create<Dictionary<string, RegisteredUserDto>>
                        (ResponseHttpType.Success, "User has been successfully created.", resultData);

                    return Ok(response);
                }

                var conflictError = result.Errors.FirstOrDefault(e => e.Code == "DuplicateUserName");
                if (conflictError != null)
                {
                    var response = _responseFactory.Create
                        (ResponseHttpType.Conflict, "User with this username already exists.");

                    return Conflict(response);
                }

                var responseBadRequest = _responseFactory.Create
                    (ResponseHttpType.BadRequest, "Invalid register attempt.");

                return BadRequest(responseBadRequest);
            }
            catch (Exception ex)
            {
                var response = _responseFactory.Create
                    (ResponseHttpType.InternalServerError, "An internal server error occurred.");

                return StatusCode(500, response);
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(loginDto.UserName);
                if (user == null)
                {
                    var response = _responseFactory.Create
                        (ResponseHttpType.NotFound, "A user with this username does not exist.");

                    return NotFound(response);
                }

                var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

                if (result.Succeeded)
                {
                    var refreshToken = await _tokenManager.CreateAccessTokenAsync(user);
                    var accessToken = await _tokenManager.CreateAccessTokenAsync(user);

                    if (refreshToken.IsSuccess && accessToken.IsSuccess)
                    {
                        var resultData = new Dictionary<string, LoggedUserDto>
                            {
                                { "user", new LoggedUserDto()
                                    {
                                        UserName = loginDto.UserName,
                                        Id = user.Id,
                                        AccessToken = accessToken.Value,
                                        RefreshToken = refreshToken.Value
                                    }
                                }
                            };

                        var response = _responseFactory.Create<Dictionary<string, LoggedUserDto>>
                            (ResponseHttpType.Success, "The user has been successfully logged in.", resultData);

                        return Ok(response);
                    }


                }

                var responseBadRequest = _responseFactory.Create
                    (ResponseHttpType.BadRequest, "Invalid login attempt.");

                return BadRequest(responseBadRequest);
            }
            catch (Exception ex)
            {
                var response = _responseFactory.Create
                    (ResponseHttpType.InternalServerError, "An internal server error occurred.");

                return StatusCode(500, response);
            }
        }


        //[HttpPost("refreshAccessToken")]
        //public async Task<IActionResult> refreshAccessToken([FromBody] string refreshToken)
        //{

        //    if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(refreshToken))
        //    {
        //        return BadRequest(new RefreshAccessTokenResponseDto
        //        {
        //            Succeeded = false,
        //            Message = "Refresh token must not be null or empty."
        //        });
        //    }

        //    var newToken = await _tokenManager.RefreshAccessTokenAsync(refreshToken);

        //    if(newToken.IsSuccess)
        //    {
        //        var response = new RefreshAccessTokenResponseDto()
        //        {
        //            Succeeded = true,
        //            Message = "The access token have been successfully refreshed.",
        //            AccessToken = newToken.Value
        //        };
        //        return Ok(response);
        //    }

        //    return BadRequest();


        //}
    }
}
