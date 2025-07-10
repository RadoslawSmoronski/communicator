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
        private readonly ILogger<UserController> _logger;

        public UserController(ITokenService tokenManager, IAccountManager accountManager, ILogger<UserController> logger)
        {
            _tokenManager = tokenManager;
            _accountManager = accountManager;
            _logger = logger;
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
                _logger.LogInformation("[RegisterAsync] User registered successfully. Username: {Username}", registerDto.UserName);
                return StatusCode(201, result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Conflict)
                {
                    _logger.LogWarning("[RegisterAsync] Registration conflict. Username: {Username}. Message: {ErrorMessage}",
                        registerDto.UserName, errorMessage);
                    return Problem(
                        statusCode: 409,
                        title: "Conflict",
                        detail: "A user with this username already exists."
                    );
                }

                _logger.LogError("[RegisterAsync] Unexpected error during registration. Username: {Username}. ErrorType: {ErrorType}. Message: {ErrorMessage}",
                    registerDto.UserName, errorCode, errorMessage);
                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: errorMessage
                );
            }

            _logger.LogError("[RegisterAsync] Unexpected error with null error object. Username: {Username}", registerDto.UserName);
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
                _logger.LogInformation("[LoginAsync] User logged in successfully. Username: {Username}", loginDto.UserName);
                return Ok(result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Unauthorized)
                {
                    _logger.LogWarning("[LoginAsync] Unauthorized login attempt. Username: {Username}. Message: {ErrorMessage}",
                        loginDto.UserName, errorMessage);
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "Username or password is incorrect."
                    );
                }

                _logger.LogError("[LoginAsync] Unexpected error during login. Username: {Username}. ErrorType: {ErrorType}. Message: {ErrorMessage}",
                    loginDto.UserName, errorCode, errorMessage);
                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: errorMessage
                );
            }

            _logger.LogError("[LoginAsync] Unexpected error with null error object. Username: {Username}", loginDto.UserName);
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
                _logger.LogInformation("[RefreshAccessTokenAsync] Access token refreshed successfully.");
                return Ok(newToken.Value);
            }
            else if (newToken.Error != null)
            {
                if (newToken.Error.ErrorType == ErrorType.Validation)
                {
                    _logger.LogWarning("[RefreshAccessTokenAsync] Invalid refresh token provided.");
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: "The provided refresh token is invalid or malformed."
                    );
                }
                else if (newToken.Error.ErrorType == ErrorType.Unauthorized)
                {
                    _logger.LogWarning("[RefreshAccessTokenAsync] Unauthorized attempt to refresh token.");
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "Refreshing access token validation failed due to unauthorized access. Please log in again."
                    );
                }

                _logger.LogError("[RefreshAccessTokenAsync] Unexpected error during token refresh. ErrorType: {ErrorType}. Message: {ErrorMessage}",
                    newToken.Error.ErrorType, newToken.Error.Description);
                return Problem(
                    statusCode: 500,
                    title: "Unexpected refreshing access token failure",
                    detail: "Refreshing access token failed unexpectedly. Please try again later or contact support."
                );
            }

            _logger.LogError("[RefreshAccessTokenAsync] Unexpected error with null error object.");
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

            _logger.LogInformation("[ChangeUsernameAsync] Attempting to change username. UserId: {UserId}, NewUsername: {NewUsername}", userId, newUsername);

            var result = await _accountManager.ChangeUsernameAsync(userId, newUsername);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[ChangeUsernameAsync] Username changed successfully. UserId: {UserId}, NewUsername: {NewUsername}", userId, newUsername);
                return Ok(result.Value);
            }
            else if (result.Error != null)
            {
                if (result.Error.ErrorType == ErrorType.Validation && result.Error.Code == "VALIDATION")
                {
                    _logger.LogWarning("[ChangeUsernameAsync] Validation error: username is empty or null. UserId: {UserId}", userId);
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: "Username cannot be empty or null."
                    );
                }
                else if (result.Error.ErrorType == ErrorType.Validation && result.Error.Code == "VALIDATION_USERID")
                {
                    _logger.LogWarning("[ChangeUsernameAsync] Validation error: unable to extract user ID from token.");
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "Unable to extract user ID from the access token. Please log in again."
                    );
                }
                else if (result.Error.ErrorType == ErrorType.Unauthorized)
                {
                    _logger.LogWarning("[ChangeUsernameAsync] Unauthorized: user not found. UserId: {UserId}", userId);
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "The user associated with the access token does not exist. Please log in again."
                    );
                }
                else if (result.Error.ErrorType == ErrorType.Conflict)
                {
                    _logger.LogWarning("[ChangeUsernameAsync] Username conflict. UserId: {UserId}, NewUsername: {NewUsername}", userId, newUsername);
                    return Problem(
                        statusCode: 409,
                        title: "Conflict",
                        detail: "The chosen username is already taken. Please choose a different one."
                    );
                }

                _logger.LogError("[ChangeUsernameAsync] Unexpected error. UserId: {UserId}. ErrorType: {ErrorType}, Message: {ErrorMessage}",
                    userId, result.Error.ErrorType, result.Error.Description);
                return Problem(
                    statusCode: 500,
                    title: "Unexpected server error.",
                    detail: "Unexpected server error."
                );
            }

            _logger.LogError("[ChangeUsernameAsync] Unexpected error with null error object. UserId: {UserId}", userId);
            return Problem(
                statusCode: 500,
                title: "Unexpected server error",
                detail: "An unexpected error occurred."
            );
        }

        [HttpPost("avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Ok();
        }

    }
}