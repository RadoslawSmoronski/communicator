using ChatCommunicator.API.Controllers;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using ChatCommunicator.Contracts.Dtos.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatCommunicator.Application.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly ITokenService _tokenManager;
        private readonly IAccountManager _accountManager;
        private readonly ILogger<UserController> _logger;

        public UserController(ITokenService tokenManager,
            IAccountManager accountManager,
            ILogger<UserController> logger)
        {
            _tokenManager = tokenManager;
            _accountManager = accountManager;
            _logger = logger;
        }

        /// <summary>
        /// Register
        /// </summary>
        /// <remarks>
        /// This endpoint creates a new user using an email, username, and password. If the email already exists,
        /// a conflict response is returned. On success, basic user data is returned.
        /// </remarks>
        /// <param name="registerDto">The registration data including email, username, and password.</param>
        /// <returns>A response containing the created user's ID and username, or an error message.</returns>
        /// <response code="201">User successfully created.</response>
        /// <response code="400">Invalid registration data (e.g., password policy not met).</response>
        /// <response code="409">Email already exists.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// POST /api/user/register
        /// {
        ///     "email": "email@email.com",
        ///     "username": "usernameTest",
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
                _logger.LogInformation("[RegisterAsync] User registered successfully. Email: {Email}", registerDto.Email);
                return StatusCode(201, result.Value);
            }

            return HandleError(result, "RegisterAsync", _logger);
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
                _logger.LogInformation("[LoginAsync] User logged in successfully. Email: {Email}", loginDto.Email);
                return Ok(result.Value);
            }

            return HandleError(result, "LoginAsync", _logger);
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

            return HandleError(newToken, "RefreshAccessTokenAsync", _logger);
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
            var validate = ValidateAndGetUserId("ChangeUsernameAsync", _logger, out Guid userId);

            if (validate != null)
            {
                return validate;
            }

            _logger.LogInformation("[ChangeUsernameAsync] Attempting to change username. UserId: {UserId}, NewUsername: {NewUsername}", userId, newUsername);

            var result = await _accountManager.ChangeUsernameAsync(userId, newUsername);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[ChangeUsernameAsync] Username changed successfully. UserId: {UserId}, NewUsername: {NewUsername}", userId, newUsername);
                return Ok(result.Value);
            }

            return HandleError(result, "ChangeUsernameAsync", _logger);
        }

        /// <summary>
        /// Change Password
        /// </summary>
        /// <remarks>
        /// Authenticated users can change their password by providing the old password and a new password.
        /// The new password must meet the required criteria.
        /// </remarks>
        /// <param name="changePasswordDto">The DTO containing the old and new passwords.</param>
        /// <returns>A success response or a detailed error response.</returns>
        /// <response code="200">Password successfully changed.</response>
        /// <response code="400">Invalid input (e.g., missing user ID, invalid old or new password).</response>
        /// <response code="401">User is not authenticated or token is invalid.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// PATCH /api/user/change-password
        /// Authorization: Bearer {token}
        /// {
        ///     "oldPassword": "OldPassword123!",
        ///     "newPassword": "NewPassword456!"
        /// }
        /// </code>
        /// </example>
        [Authorize]
        [HttpPatch("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePasswordAsync([FromQuery] ChangePasswordDto changePasswordDto)
        {
            var validate = ValidateAndGetUserId("ChangePasswordAsync", _logger, out Guid userId);

            if (validate != null)
            {
                return validate;
            }

            var result = await _accountManager.ChangePasswordAsync(userId, changePasswordDto.OldPassword, changePasswordDto.NewPassword);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[ChangePasswordAsync] Password changed successfully for user {UserId}.", userId);
                return Ok();
            }

            return HandleError(result, "ChangePasswordAsync", _logger);
        }

        /// <summary>
        /// Upload Avatar
        /// </summary>
        /// <remarks>
        /// Authenticated users can upload a new avatar image. 
        /// The uploaded file must meet specific requirements:
        /// <br/>- Maximum file size: 5 MB.
        /// <br/>- Maximum dimensions: 500x500 pixels.
        /// <br/>- Supported formats: JPEG, PNG, etc.
        /// </remarks>
        /// <param name="uploadAvatarDto">Form data containing the avatar file.</param>
        /// <returns>URL of the uploaded avatar or a detailed error response.</returns>
        /// <response code="200">Avatar successfully uploaded and URL returned.</response>
        /// <response code="400">Invalid file (e.g., empty, too big, wrong format, too large, too small).</response>
        /// <response code="401">User is not authenticated or token is invalid.</response>
        /// <response code="404">User not found in the system.</response>
        /// <response code="409">User already has an avatar set.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// POST /api/user/avatar
        /// Authorization: Bearer {token}
        /// Content-Type: multipart/form-data
        /// 
        /// Form Data:
        /// file: avatar_image.png
        /// </code>
        /// </example>
        [Authorize]
        [HttpPost("avatar")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadAvatarAsync([FromForm] UploadAvatarDto uploadAvatarDto)
        {
            var validate = ValidateAndGetUserId("UploadAvatarAsync", _logger, out Guid userId);

            if (validate != null)
            {
                return validate;
            }

            var result = await _accountManager.UploadAvatarAsync(userId, uploadAvatarDto.File);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleError(result, "UploadAvatarAsync", _logger);
        }

        /// <summary>
        /// Delete Avatar
        /// </summary>
        /// <remarks>
        /// Authenticated users can delete their avatar. 
        /// This action removes the avatar associated with the user account.
        /// </remarks>
        /// <returns>A success response or a detailed error response.</returns>
        /// <response code="200">Avatar successfully deleted.</response>
        /// <response code="400">Invalid or missing user ID.</response>
        /// <response code="404">User not found in the system.</response>
        /// <response code="409">No avatar is set for this user, so there is nothing to delete.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// DELETE /api/user/avatar
        /// Authorization: Bearer {token}
        /// </code>
        /// </example>
        [Authorize]
        [HttpDelete("avatar")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAvatarAsync()
        {
            var validate = ValidateAndGetUserId("DeleteAvatarAsync", _logger, out Guid userId);

            if (validate != null)
            {
                return validate;
            }

            var result = await _accountManager.DeleteAvatarAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[DeleteAvatarAsync] Avatar deleted successfully for user {UserId}.", userId);
                return Ok();
            }

            return HandleError(result, "DeleteAvatarAsync", _logger);
        }

        /// <summary>
        /// Change Avatar
        /// </summary>
        /// <remarks>
        /// Authenticated users can change their avatar by uploading a new image. 
        /// The uploaded file must meet specific requirements:
        /// <br/>- Maximum file size: 5 MB.
        /// <br/>- Maximum dimensions: 500x500 pixels.
        /// <br/>- Supported formats: JPEG, PNG, etc.
        /// </remarks>
        /// <param name="uploadAvatarDto">Form data containing the new avatar file.</param>
        /// <returns>URL of the updated avatar or a detailed error response.</returns>
        /// <response code="200">Avatar successfully updated and URL returned.</response>
        /// <response code="400">Invalid file (e.g., empty, too big, wrong format, too large, too small).</response>
        /// <response code="401">User is not authenticated or token is invalid.</response>
        /// <response code="404">User not found in the system.</response>
        /// <response code="409">User does not have an avatar set to change.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// PUT /api/user/avatar
        /// Authorization: Bearer {token}
        /// Content-Type: multipart/form-data
        /// 
        /// Form Data:
        /// file: new_avatar_image.png
        /// </code>
        /// </example>
        [Authorize]
        [HttpPut("avatar")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeAvatarAsync([FromForm] UploadAvatarDto uploadAvatarDto)
        {
            var validate = ValidateAndGetUserId("ChangeAvatarAsync", _logger, out Guid userId);

            if (validate != null)
            {
                return validate;
            }

            var result = await _accountManager.ChangeAvatarAsync(userId, uploadAvatarDto.File);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleError(result, "ChangeAvatarAsync", _logger);
        }

    }
}