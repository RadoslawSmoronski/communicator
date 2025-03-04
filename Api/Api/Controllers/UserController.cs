using Api.Managers.Interfaces;
using Api.Models;
using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
using Api.Models.Dtos.Controllers.UserController.LoginAsync;
using Api.Models.Dtos.Responses;
using Api.Models.Dtos.Responses.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Api.Models.Dtos.Controllers.UserController;

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
                        (ResponseHttpType.Success, "The user has been successfully created.", resultData);

                    return Ok(response);
                }

                var conflictError = result.Errors.FirstOrDefault(e => e.Code == "DuplicateUserName");
                if (conflictError != null)
                {
                    var response = _responseFactory.Create
                        (ResponseHttpType.Conflict, "A user with this username already exists.");

                    return Conflict(response);
                }

                var responseBadRequest = _responseFactory.Create
                    (ResponseHttpType.BadRequest, "Invalid registration attempt.");

                return BadRequest(responseBadRequest);
            }
            catch (Exception)
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
                        (ResponseHttpType.NotFound, "No user with this username exists.");

                    return NotFound(response);
                }

                var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

                if (result.Succeeded)
                {
                    var refreshToken = await _tokenManager.CreateRefreshTokenAsync(user.Id);
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
            catch (Exception)
            {
                var response = _responseFactory.Create
                    (ResponseHttpType.InternalServerError, "An internal server error occurred.");

                return StatusCode(500, response);
            }
        }


        [HttpPost("refreshAccessToken")]
        public async Task<IActionResult> RefreshAccessTokenAsync([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var newToken = await _tokenManager.RefreshAccessTokenAsync(refreshTokenDto.RefreshToken);

            if (newToken.IsSuccess)
            {
                var response = _responseFactory.Create<String>
                    (ResponseHttpType.Success, "The access token has been successfully refreshed.", newToken.Value);
                
                return Ok(response);
            }
            else if(newToken.Error != null)
            {
                var error = newToken.Error;

                var errorResponseType = _mapper.Map<ResponseHttpType>(error.ErrorType);

                var errorResponse = _responseFactory.Create(errorResponseType, error.Description);

                return StatusCode(errorResponse.Status, errorResponse);
            }

            var response500 = _responseFactory.Create
                (ResponseHttpType.InternalServerError, "An internal server error occurred.");

            return StatusCode(500, response500);
        }
    }
}
