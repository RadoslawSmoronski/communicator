using ChatCommunicator.Application.Controllers;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using ChatCommunicator.Contracts.Dtos.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ChatCommunicator.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly ITokenService _tokenManager;
        private readonly IAccountManager _accountManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ITokenService tokenManager,
            IAccountManager accountManager,
            ILogger<AuthController> logger)
        {
            _tokenManager = tokenManager;
            _accountManager = accountManager;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates user
        /// </summary>
        /// <remarks>
        /// Validates the provided login credentials. On success, returns a JWT access token and a refresh token.
        /// If authentication fails, a detailed error is returned.
        /// </remarks>
        /// <param name="loginDto">The login credentials, including email and password.</param>
        /// <returns>A <see cref="LoggedUserDto"/> object if authentication is successful, or an error response otherwise.</returns>
        /// <response code="200">User successfully authenticated. Tokens returned.</response>
        /// <response code="400">Invalid login request (e.g., malformed input).</response>
        /// <response code="401">Invalid email or password.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// POST /api/auth/login
        /// {
        ///     "email": "user@example.com",
        ///     "password": "UserPassword123!"
        /// }
        /// </example>
        [HttpPost("login")]
        [ProducesResponseType<LoggedUserDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            var result = await _accountManager.LoginAsync(loginDto);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[LoginAsync] User logged in successfully. Email: {Email}", loginDto.Email);
                return Ok(result.Value);
            }

            return HandleError(result, "LoginAsync", _logger);
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <remarks>
        /// Validates the provided refresh token and, if valid, issues a new access token and refresh token.
        /// </remarks>
        /// <param name="refreshTokenDto">The DTO containing the refresh token.</param>
        /// <returns>A <see cref="RefreshAccessTokenDto"/> with new tokens if successful, or an error response otherwise.</returns>
        /// <response code="200">Access token successfully refreshed.</response>
        /// <response code="400">Invalid or malformed refresh token.</response>
        /// <response code="401">Unauthorized or expired refresh token.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// POST /api/auth/refresh-token
        /// {
        ///     "refreshToken": "00000000-0000-0000-0000-000000000000"
        /// }
        /// </example>
        [HttpPost("refresh-token")]
        [ProducesResponseType<RefreshAccessTokenDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshAccessTokenAsync([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var newToken = await _tokenManager.RefreshAccessTokenAsync(refreshTokenDto.RefreshToken);

            if (newToken.IsSuccess)
            {
                _logger.LogInformation("[RefreshAccessTokenAsync] Access token refreshed successfully.");
                return Ok(newToken.Value);
            }

            return HandleError(newToken, "RefreshAccessTokenAsync", _logger);
        }

        /// <summary>
        /// Confirms user's email address
        /// </summary>
        /// <remarks>
        /// Validates the provided confirmation token for the specified user and confirms their email address.
        /// <b>Note:</b> This endpoint is currently not fully finished, but is functional.
        /// </remarks>
        /// <param name="confirmEmailDto">The DTO containing the user's ID and confirmation token.</param>
        /// <returns>
        /// <see cref="IActionResult"/> indicating the result of the confirmation attempt.
        /// Returns <c>200 OK</c> if the email was confirmed successfully, or <c>400 Bad Request</c> if confirmation failed.
        /// </returns>
        /// <response code="200">Email confirmed successfully.</response>
        /// <response code="400">Invalid confirmation token or user ID.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// POST /api/auth/confirm-email
        /// {
        ///     "userId": "00000000-0000-0000-0000-000000000000",
        ///     "token": "confirmation-token-value"
        /// }
        /// </example>
        [HttpPost("confirm-email")]
        [ProducesResponseType<RefreshAccessTokenDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailDto confirmEmailDto)
        {
            var result = await _accountManager.ConfirmEmailAsync(confirmEmailDto);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}
