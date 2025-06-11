using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController;
using ChatCommunicator.Contracts.Dtos.Service;
using ChatCommunicator.Contracts.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ChatCommunicator.Shared.Result;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Application.Managers;
using ChatCommunicator.Application.Services.Interfaces;

namespace ChatCommunicator.Application.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenManager;
        private readonly IAccountManager _accountManager;

        public UserController(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager,
            IMapper mapper, ITokenService tokenManager, IAccountManager accountManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _tokenManager = tokenManager;
            _accountManager = accountManager;
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
            var result = await _accountManager.RegisterAsync(registerDto);

            if (result.IsSuccess)
            {
                return Ok(result.Value); // todo: zmienić na created
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
                        detail: errorMessage // todo: zmienic
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
        /// Change username
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to update their username. It verifies whether the new username
        /// is already taken and returns a conflict response if so. The user ID is extracted from the JWT token.
        /// </remarks>
        /// <param name="newUsername">The new username to assign to the current user.</param>
        /// <returns>
        /// A response indicating whether the username was successfully updated or an appropriate error message.
        /// </returns>
        /// <response code="200">Username successfully updated. Returns the new username.</response>
        /// <response code="400">The provided username is null, empty, or invalid.</response>
        /// <response code="401">The access token is missing, invalid, or refers to a non-existent user.</response>
        /// <response code="409">The desired username is already taken by another user.</response>
        /// <response code="500">An unexpected server error occurred while updating the username.</response>
        /// <example>
        /// <code>
        /// PATCH /api/changeUsername?newUsername=new_name_123
        /// Authorization: Bearer {token}
        /// </code>
        /// </example>
        [Authorize]
        [HttpPatch("changeUsername")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeUsernameAsync([FromQuery] string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
            {
                return Problem(
                    statusCode: 400,
                    title: "Bad Request",
                    detail: "Username cannot be empty or null."
                );
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if(userId == null)
                {
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "Unable to extract user ID from the access token. Please log in again."
                    );
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "The user associated with the access token does not exist. Please log in again."
                    );
                }

                var isUsernameExists = await _userManager.FindByNameAsync(newUsername);

                if (isUsernameExists != null)
                {
                    return Problem(
                        statusCode: 409,
                        title: "Conflict",
                        detail: "The chosen username is already taken. Please choose a different one."
                    );
                }

                var result = await _userManager.SetUserNameAsync(user, newUsername);

                if (result.Succeeded)
                {
                    return Ok(newUsername);
                }

                return Problem(
                    statusCode: 500,
                    title: "Unexpected username change failure",
                    detail: "An unexpected error occurred while attempting to change the username. Please try again later or contact support."
                );
            }
            catch
            {
                return Problem(
                    statusCode: 500,
                    title: "Unexpected server error",
                    detail: "An unexpected error occurred during refreshing access token."
                );
            }
        }
    }
}