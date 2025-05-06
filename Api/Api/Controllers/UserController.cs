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
using Api.Models.Dtos.Service;
using Api.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Api.Utilities.Result;

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

        /// <summary>
        /// Register.
        /// </summary>
        /// <remarks>
        /// This endpoint creates a new user using a username and password. If the username already exists,
        /// a conflict response is returned. On success, basic user data is returned.
        /// </remarks>
        /// <param name="registerDto">The registration data including username and password.</param>
        /// <returns>
        /// A response containing the created user's ID and username, or an error message.
        /// </returns>
        /// <response code="201">User successfully created.</response>
        /// <response code="400">Invalid registration data (e.g., password policy not met).</response>
        /// <response code="409">Username already exists.</response>
        /// <response code="500">Unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// POST /api/user/register
        /// {
        ///     "userName": "newuser123",
        ///     "password": "StrongPassword123!"
        /// }
        /// </code>
        /// </example>
        [HttpPost("register")]
        [ProducesResponseType(typeof(SimpleUserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            try
            {
                var user = new UserAccount { UserName = registerDto.UserName };
                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    var dto = _mapper.Map<SimpleUserDto>(user);
                    return Created(string.Empty, dto);
                }

                var conflictError = result.Errors.FirstOrDefault(e => e.Code == "DuplicateUserName");
                if (conflictError != null)
                {
                    return Problem(
                        statusCode: 409,
                        title: "Conflict",
                        detail: "A user with this username already exists.",
                        instance: HttpContext.Request.Path
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "Unexpected registration failure",
                    detail: "User registration failed unexpectedly. Please try again later or contact support.",
                    instance: HttpContext.Request.Path
                );
            }
            catch (Exception)
            {
                return Problem(
                    statusCode: 500,
                    title: "Unexpected server error",
                    detail: "An unexpected error occurred during user registration.",
                    instance: HttpContext.Request.Path
                );
            }
        }

        /// <summary>
        /// Login.
        /// </summary>
        /// <remarks>
        /// This endpoint validates the provided login credentials. If authentication is successful, it returns a JWT access token
        /// and a refresh token for session management. If the user does not exist or the credentials are invalid, an appropriate error is returned.
        /// </remarks>
        /// <param name="loginDto">The login credentials, including username and password.</param>
        /// <returns>
        /// A <see cref="LoggedUserDto"/> object containing the authenticated user's ID, username, access token, and refresh token,
        /// or a <see cref="ProblemDetails"/> response in case of failure.
        /// </returns>
        /// <response code="200">User successfully authenticated. Tokens returned.</response>
        /// <response code="400">Invalid login request (e.g., malformed input).</response>
        /// <response code="401">Invalid username or password.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// POST /api/login
        /// {
        ///     "userName": "existinguser",
        ///     "password": "UserPassword123!"
        /// }
        /// </example>
        [HttpPost("login")]
        [ProducesResponseType<LoggedUserDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(loginDto.UserName);

                if (user == null)
                {
                    return Problem(
                        statusCode: 401,
                        title: "Invalid credentials",
                        detail: "Username or password is incorrect.",
                        instance: HttpContext.Request.Path
                    );
                }

                var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);

                if (result.Succeeded)
                {
                    var refreshToken = await _tokenManager.CreateRefreshTokenAsync(user.Id);
                    var accessToken = await _tokenManager.CreateAccessTokenAsync(user);

                    if (refreshToken.IsSuccess && accessToken.IsSuccess)
                    {
                        var resultObj = new LoggedUserDto()
                        {
                            UserName = loginDto.UserName,
                            Id = user.Id,
                            AccessToken = accessToken.Value,
                            RefreshToken = refreshToken.Value
                        };

                        return Ok(resultObj);
                    }
                }

                return Problem(
                    statusCode: 500,
                    title: "Unexpected logging failure",
                    detail: "User logging failed unexpectedly. Please try again later or contact support.",
                    instance: HttpContext.Request.Path
                );
            }
            catch (Exception)
            {
                return Problem(
                    statusCode: 500,
                    title: "Unexpected server error",
                    detail: "An unexpected error occurred during user logging.",
                    instance: HttpContext.Request.Path
                );
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <remarks>
        /// This endpoint validates the provided refresh token and, if valid, issues a new access token.
        /// It is typically used when the current access token has expired but the refresh token is still valid.
        /// </remarks>
        /// <param name="refreshTokenDto">The object containing the refresh token string.</param>
        /// <returns>
        /// A new access token if the refresh token is valid; otherwise, a problem detail describing the failure.
        /// </returns>
        /// <response code="200">Access token successfully refreshed.</response>
        /// <response code="400">The refresh token is invalid or malformed (e.g., structurally incorrect).</response>
        /// <response code="401">The refresh token is valid in format but unauthorized (e.g., expired, revoked, or forged).</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// POST /api/refreshAccessToken
        /// {
        ///     "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        /// }
        /// </code>
        /// </example>
        [HttpPost("refreshAccessToken")]
        [ProducesResponseType<RefreshAccessTokenDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshAccessTokenAsync([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var newToken = await _tokenManager.RefreshAccessTokenAsync(refreshTokenDto.RefreshToken);

            if (newToken.IsSuccess)
            {       
                return Ok(newToken.Value);
            }
            else if(newToken.Error != null)
            {
                if(newToken.Error.ErrorType == ErrorType.Validation)
                {
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: "The provided refresh token is invalid or malformed.",
                        instance: HttpContext.Request.Path
                    );
                }
                else if(newToken.Error.ErrorType == ErrorType.Unauthorized)
                {
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "Refreshing access token validation failed due to unauthorized access. Please log in again.",
                        instance: HttpContext.Request.Path
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "Unexpected refreshing access token failure",
                    detail: "Refreshing access token failed unexpectedly. Please try again later or contact support.",
                    instance: HttpContext.Request.Path
                );
            }

            return Problem(
                statusCode: 500,
                title: "Unexpected server error",
                detail: "An unexpected error occurred during refreshing access token.",
                instance: HttpContext.Request.Path
            );
        }

        // REFACTOR - Refactor this – quick & dirty implementation
        /// <summary>
        /// changeUsername [Dirty endpoint to refactor but it workings.]
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to update their username. It verifies whether the new username
        /// is already taken and returns a conflict response if so. The user ID is extracted from the JWT token.
        /// </remarks>
        /// <param name="newUsername">The new username to assign to the current user.</param>
        /// <returns>
        /// A response indicating whether the username was successfully updated or an appropriate error message.
        /// </returns>
        /// <response code="204">Username successfully updated. No content returned.</response>
        /// <response code="400">Invalid request or update failed.</response>
        /// <response code="409">Username already exists.</response>
        /// <response code="500">Unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// PATCH /api/changeUsername?newUsername=new_name_123
        /// Authorization: Bearer {token}
        /// </code>
        /// </example>
        [Authorize]
        [HttpPatch("changeUsername")]
        public async Task<IActionResult> ChangeUsernameAsync([FromQuery] string newUsername)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if(userId == null)
                {
                    var response = _responseFactory.Create
                        (ResponseHttpType.InternalServerError, "To edit.");

                    return StatusCode(response.Status, response);
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    var response = _responseFactory.Create
                        (ResponseHttpType.InternalServerError, "To edit.");

                    return StatusCode(response.Status, response);
                }

                var isUsernameExists = await _userManager.FindByNameAsync(newUsername);

                if (isUsernameExists != null)
                {   
                    var response = _responseFactory.Create
                        (ResponseHttpType.Conflict, "To edit.");

                    return Conflict(response);
                }

                var result = await _userManager.SetUserNameAsync(user, newUsername);
                //return result.Succeeded ? NoContent() : BadRequest(result.Errors);

                //return  CreatedAtAction(nameof(CreateScheduleItem), new { scheduleItem.ScheduleItemId }, scheduleItem);
                return Ok(result);
            }
            catch
            {
                var response = _responseFactory.Create
                                (ResponseHttpType.InternalServerError, "To edit.");

                return StatusCode(response.Status, response);
            }

        }
    }
}
