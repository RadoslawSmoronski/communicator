using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using Microsoft.AspNetCore.Mvc;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController;
using ChatCommunicator.Contracts.Dtos.Service;
using ChatCommunicator.Contracts.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChatCommunicator.Shared.Result;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;

namespace ChatCommunicator.Application.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly ITokenService _tokenManager;
        private readonly IAccountManager _accountManager;

        public UserController(ITokenService tokenManager, IAccountManager accountManager)
        {
            _tokenManager = tokenManager;
            _accountManager = accountManager;
        }

        /// <summary>
        /// Register
        /// </summary>
        /// <remarks>
        /// This endpoint creates a new user using a username and password. If the username already exists,
        /// a conflict response is returned. On success, basic user data is returned.
        /// </remarks>
        /// <param name="registerDto">The registration data including username and password.</param>
        /// <returns>A response containing the created user's ID and username, or an error message.</returns>
        /// <response code="201">User successfully created.</response>
        /// <response code="400">Invalid registration data (e.g., password policy not met).</response>
        /// <response code="409">Username already exists.</response>
        /// <response code="500">An unexpected server error occurred.</response>
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
            var result = await _accountManager.RegisterAsync(registerDto);

            if (result.IsSuccess)
            {
                return StatusCode(201, result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Conflict)
                {
                    return Problem(
                        statusCode: 409,
                        title: "Conflict",
                        detail: "A user with this username already exists."
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: errorMessage
                );
            }

            return Problem(
                statusCode: 500,
                title: "Unexpected server error",
                detail: "An unexpected error occurred during user registration."
            );
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <remarks>
        /// This endpoint validates the provided login credentials. On success, it returns a JWT access token
        /// and a refresh token. If authentication fails, a detailed error is returned.
        /// </remarks>
        /// <param name="loginDto">The login credentials, including username and password.</param>
        /// <returns>A token object if authentication is successful or an error response otherwise.</returns>
        /// <response code="200">User successfully authenticated. Tokens returned.</response>
        /// <response code="400">Invalid login request (e.g., malformed input).</response>
        /// <response code="401">Invalid username or password.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// POST /api/user/login
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
            var result = await _accountManager.LoginAsync(loginDto);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Unauthorized)
                {
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "Username or password is incorrect."
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: errorMessage
                );
            }

            return Problem(
                statusCode: 500,
                title: "Unexpected server error",
                detail: "An unexpected error occurred during user registration."
            );
        }

        /// <summary>
        /// Refresh Access Token
        /// </summary>
        /// <remarks>
        /// This endpoint checks the provided refresh token and issues a new access token if it's valid.
        /// </remarks>
        /// <param name="refreshTokenDto">The refresh token container.</param>
        /// <returns>New access token or error response.</returns>
        /// <response code="200">Access token successfully refreshed.</response>
        /// <response code="400">Invalid or malformed refresh token.</response>
        /// <response code="401">Unauthorized or expired refresh token.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// POST /api/user/refresh-access-token
        /// {
        ///     "refreshToken": "your_refresh_token_here"
        /// }
        /// </code>
        /// </example>
        [HttpPost("refresh-access-token")]
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
                        detail: "The provided refresh token is invalid or malformed."
                    );
                }
                else if(newToken.Error.ErrorType == ErrorType.Unauthorized)
                {
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "Refreshing access token validation failed due to unauthorized access. Please log in again."
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "Unexpected refreshing access token failure",
                    detail: "Refreshing access token failed unexpectedly. Please try again later or contact support."
                );
            }

            return Problem(
                statusCode: 500,
                title: "Unexpected server error",
                detail: "An unexpected error occurred during refreshing access token."
            );
        }

        /// <summary>
        /// Change Username
        /// </summary>
        /// <remarks>
        /// Authenticated users can change their username. The new username must be unique.
        /// </remarks>
        /// <param name="newUsername">The new username to assign.</param>
        /// <returns>The new username or a detailed error response.</returns>
        /// <response code="200">Username successfully updated.</response>
        /// <response code="400">Invalid or missing username.</response>
        /// <response code="401">User is not authenticated or token is invalid.</response>
        /// <response code="409">The username is already taken.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// PATCH /api/user/change-username?newUsername=new_name_123
        /// Authorization: Bearer {token}
        /// </code>
        /// </example>
        [Authorize]
        [HttpPatch("change-username")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeUsernameAsync([FromQuery] string newUsername)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _accountManager.ChangeUsernameAsync(userId, newUsername);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else if (result.Error != null)
            {
                if (result.Error.ErrorType == ErrorType.Validation && result.Error.Code == "VALIDATION")
                {
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: "Username cannot be empty or null."
                    );
                }
                else if (result.Error.ErrorType == ErrorType.Validation && result.Error.Code == "VALIDATION_USERID")
                {
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "Unable to extract user ID from the access token. Please log in again."
                    );
                }
                else if (result.Error.ErrorType == ErrorType.Unauthorized)
                {
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "The user associated with the access token does not exist. Please log in again."
                    );
                }
                else if (result.Error.ErrorType == ErrorType.Conflict)
                {
                    return Problem(
                        statusCode: 409,
                        title: "Conflict",
                        detail: "The chosen username is already taken. Please choose a different one."
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "Unexpected server error.",
                    detail: "Unexpected server error."
                    );
            }

            return Problem(
                statusCode: 500,
                title: "Unexpected server error",
                detail: "An unexpected error occurred."
            );
        }
    }
}