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
using ChatCommunicator.API.Controllers;

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

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Conflict)
                {
                    _logger.LogWarning("[RegisterAsync] Registration conflict. Email: {Email}. Message: {ErrorMessage}",
                        registerDto.Email, errorMessage);
                    return Problem(
                        statusCode: 409,
                        title: "Conflict",
                        detail: "A user with this email already exists."
                    );
                }

                _logger.LogError("[RegisterAsync] Unexpected error during registration. Email: {Email}. ErrorType: {ErrorType}. Message: {ErrorMessage}",
                    registerDto.Email, errorCode, errorMessage);
                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: errorMessage
                );
            }

            _logger.LogError("[RegisterAsync] Unexpected error with null error object. Email: {Email}", registerDto.Email);
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
                _logger.LogInformation("[LoginAsync] User logged in successfully. Email: {Email}", loginDto.Email);
                return Ok(result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Unauthorized)
                {
                    _logger.LogWarning("[LoginAsync] Unauthorized login attempt. Email: {Email}. Message: {ErrorMessage}",
                        loginDto.Email, errorMessage);
                    return Problem(
                        statusCode: 401,
                        title: "Unauthorized",
                        detail: "Email or password is incorrect."
                    );
                }

                _logger.LogError("[LoginAsync] Unexpected error during login. Email: {Email}. ErrorType: {ErrorType}. Message: {ErrorMessage}",
                    loginDto.Email, errorCode, errorMessage);
                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: errorMessage
                );
            }

            _logger.LogError("[LoginAsync] Unexpected error with null error object. Email: {Email}", loginDto.Email);
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
            var userId = GetUserIdByClaims();

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("[ChangeUsernameAsync] Validation error: userId from claims is empty.");
                return Problem(
                    statusCode: 400,
                    title: "Bad Request",
                    detail: "UserId cannot be empty or null."
                );
            }

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
            var userId = GetUserIdByClaims();

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("[ChangePasswordAsync] Validation error: userId from claims is empty.");
                return Problem(
                    statusCode: 400,
                    title: "Bad Request",
                    detail: "UserId cannot be empty or null."
                );
            }

            var result = await _accountManager.ChangePasswordAsync(userId, changePasswordDto.OldPassword, changePasswordDto.NewPassword);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[ChangePasswordAsync] Password changed successfully for user {UserId}.", userId);
                return Ok();
            }
            else if (result.Error != null)
            {
                var errorType = result.Error.ErrorType;
                var errorCode = result.Error.Code;

                if (errorType == ErrorType.Validation)
                {
                    switch (errorCode)
                    {
                        case "VALIDATION_USERID":
                            _logger.LogWarning("[ChangePasswordAsync] Validation error: invalid user ID.");
                            return Problem(
                                statusCode: 400,
                                title: "Bad Request",
                                detail: "Invalid user ID."
                            );

                        case "VALIDATION_OLDPASSWORD":
                            _logger.LogWarning("[ChangePasswordAsync] Validation error: old password is invalid.");
                            return Problem(
                                statusCode: 400,
                                title: "Bad Request",
                                detail: "Old password is invalid."
                            );

                        case "VALIDATION_NEWPASSWORD":
                            _logger.LogWarning("[ChangePasswordAsync] Validation error: new password is invalid.");
                            return Problem(
                                statusCode: 400,
                                title: "Bad Request",
                                detail: "New password is invalid."
                            );

                        case "VALIDATION_PASSWORDS_SAME":
                            _logger.LogWarning("[ChangePasswordAsync] Validation error: new password cannot be the same as the old password.");
                            return Problem(
                                statusCode: 400,
                                title: "Bad Request",
                                detail: "New password cannot be the same as the old password."
                            );

                        case "NEWPASSWORD_IS_NOT_VALID":
                            _logger.LogWarning("[ChangePasswordAsync] Validation error: new password does not meet the required criteria.");
                            return Problem(
                                statusCode: 400,
                                title: "Bad Request",
                                detail: "New password does not meet the required criteria."
                            );
                    }
                }
                else if (errorType == ErrorType.Unauthorized)
                {
                    switch (errorCode)
                    {
                        case "USER_NOT_FOUND":
                            _logger.LogWarning("[ChangePasswordAsync] Unauthorized error: user not found.");
                            return Problem(
                                statusCode: 401,
                                title: "Unauthorized",
                                detail: "User not found."
                            );

                        case "OLDPASSWORD_IS_INCORRECT":
                            _logger.LogWarning("[ChangePasswordAsync] Unauthorized error: old password is incorrect.");
                            return Problem(
                                statusCode: 401,
                                title: "Unauthorized",
                                detail: "Old password is incorrect."
                            );
                    }
                }
                else if (errorType == ErrorType.Unknown)
                {
                    _logger.LogError("[ChangePasswordAsync] Unknown error occurred while changing password for user {UserId}.", userId);
                    return Problem(
                        statusCode: 500,
                        title: "Internal Server Error",
                        detail: "An unknown error occurred while changing the password."
                    );
                }

                _logger.LogError("[ChangePasswordAsync] Unexpected error occurred for user {UserId}. ErrorType: {ErrorType}, ErrorCode: {ErrorCode}", userId, errorType, errorCode);
                return Problem(
                    statusCode: 500,
                    title: "Internal Server Error",
                    detail: "An unexpected error occurred while changing the password."
                );
            }

            _logger.LogError("[ChangePasswordAsync] Unexpected null error object for user {UserId}.", userId);
            return Problem(
                statusCode: 500,
                title: "Internal Server Error",
                detail: "An unexpected error occurred while changing the password."
            );
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
            var userId = GetUserIdByClaims();

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("[UploadAvatarAsync] Validation error: userId from claims is empty.");
                return Problem(
                    statusCode: 400,
                    title: "Bad Request",
                    detail: "The user ID extracted from the claims is null or empty. Please ensure you are authenticated."
                );
            }

            var result = await _accountManager.UploadAvatarAsync(userId, uploadAvatarDto.File);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else if (result.Error != null)
            {
                var error = result.Error;

                if (error.ErrorType == ErrorType.Validation)
                {
                    switch (error.Code)
                    {
                        case "FILE_IS_EMPTY":
                            _logger.LogWarning("User {UserId} tried to upload an empty file.", userId);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "The uploaded file is empty.");

                        case "INVALID_FORMAT":
                            _logger.LogWarning("User {UserId} uploaded a file with invalid format.", userId);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "The uploaded file format is not supported.");

                        case "FILE_IS_TOO_BIG":
                            _logger.LogWarning("User {UserId} uploaded a file that exceeds the maximum allowed size (weight).", userId);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "The uploaded file exceeds the allowed size limit (5 MB).");

                        case "FILE_IS_TOO_LARGE":
                            _logger.LogWarning("User {UserId} uploaded a file with dimensions larger than allowed.", userId);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "The uploaded file dimensions exceed the allowed limit (500x500 px).");


                        case "FILE_IS_TOO_SMALL":
                            _logger.LogWarning("User {UserId} uploaded a file that is too small.", userId);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "The uploaded file is too small.");

                        default:
                            _logger.LogWarning("User {UserId} upload failed due to validation error: {ErrorCode}", userId, error.Code);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "Invalid file upload.");
                    }
                }
                else if (error.ErrorType == ErrorType.NotFound)
                {
                    _logger.LogWarning("User {UserId} not found during avatar upload.", userId);
                    return Problem(statusCode: 404, title: "Not Found", detail: "The user was not found.");
                }
                else if (error.ErrorType == ErrorType.Conflict)
                {
                    _logger.LogWarning("User {UserId} tried to set avatar but avatar is already set.", userId);
                    return Problem(statusCode: 409, title: "Conflict", detail: "The user already has an avatar set.");
                }
                else if (error.ErrorType == ErrorType.Unknown && error.Code == "USER_UPDATE_FAILED")
                {
                    _logger.LogError("Failed to update avatar URL for user {UserId}.", userId);
                    return Problem(statusCode: 500, title: "Internal Server Error", detail: "Failed to update user avatar URL in database.");
                }
                else if (error.ErrorType == ErrorType.Failure)
                {
                    _logger.LogError("Failed to upload avatar for user {UserId}: {ErrorDescription}", userId, error.Description);
                    return Problem(statusCode: 500, title: "Internal Server Error", detail: "Failed to upload the avatar file.");
                }

                _logger.LogError("Unexpected error during avatar upload for user {UserId}: {ErrorCode}", userId, error.Code);
                return Problem(statusCode: 500, title: "Unexpected error", detail: "An unexpected error occurred. Please try again later or contact support.");
            }

            _logger.LogError("Unexpected null error object during avatar upload for user {UserId}", userId);
            return Problem(statusCode: 500, title: "Unexpected server error", detail: "An unexpected error occurred during avatar upload.");
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
            var userId = GetUserIdByClaims();

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("[DeleteAvatarAsync] Validation error: userId from claims is empty.");
                return Problem(
                    statusCode: 400,
                    title: "Bad Request",
                    detail: "UserId cannot be empty or null."
                );
            }

            var result = await _accountManager.DeleteAvatarAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[DeleteAvatarAsync] Avatar deleted successfully for user {UserId}.", userId);
                return Ok();
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.NotFound)
                {
                    _logger.LogWarning("[DeleteAvatarAsync] User not found. UserId: {UserId}", userId);
                    return Problem(
                        statusCode: 404,
                        title: "Not Found",
                        detail: "The user was not found."
                    );
                }
                else if (errorCode == ErrorType.Conflict)
                {
                    _logger.LogWarning("[DeleteAvatarAsync] Conflict: No avatar to delete for user {UserId}.", userId);
                    return Problem(
                        statusCode: 409,
                        title: "Conflict",
                        detail: "No avatar is set for this user, so there is nothing to delete."
                    );
                }
                else if (errorCode == ErrorType.Failure)
                {
                    _logger.LogError("[DeleteAvatarAsync] Failed to delete avatar for user {UserId}. Error: {ErrorMessage}", userId, errorMessage);
                    return Problem(
                        statusCode: 500,
                        title: "Internal Server Error",
                        detail: "Failed to delete the avatar."
                    );
                }

                _logger.LogError("[DeleteAvatarAsync] Unexpected error during avatar deletion for user {UserId}. ErrorType: {ErrorType}, Message: {ErrorMessage}",
                    userId, errorCode, errorMessage);
                return Problem(
                    statusCode: 500,
                    title: "Unexpected server error",
                    detail: "An unexpected error occurred during avatar deletion."
                );
            }

            _logger.LogError("[DeleteAvatarAsync] Unexpected null error object during avatar deletion for user {UserId}", userId);
            return Problem(
                statusCode: 500,
                title: "Unexpected server error",
                detail: "An unexpected error occurred during avatar deletion."
            );
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
            var userId = GetUserIdByClaims();

            if (userId == Guid.Empty)
            {
                _logger.LogWarning("[ChangeAvatarAsync] Validation error: userId from claims is empty.");
                return Problem(
                    statusCode: 400,
                    title: "Bad Request",
                    detail: "The user ID extracted from the claims is null or empty. Please ensure you are authenticated."
                );
            }

            var result = await _accountManager.ChangeAvatarAsync(userId, uploadAvatarDto.File);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else if (result.Error != null)
            {
                var error = result.Error;

                if (error.ErrorType == ErrorType.Validation)
                {
                    switch (error.Code)
                    {
                        case "FILE_IS_EMPTY":
                            _logger.LogWarning("User {UserId} tried to upload an empty file.", userId);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "The uploaded file is empty.");

                        case "INVALID_FORMAT":
                            _logger.LogWarning("User {UserId} uploaded a file with invalid format.", userId);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "The uploaded file format is not supported.");

                        case "FILE_IS_TOO_BIG":
                            _logger.LogWarning("User {UserId} uploaded a file that exceeds the maximum allowed size (weight).", userId);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "The uploaded file exceeds the allowed size limit (5 MB).");

                        case "FILE_IS_TOO_LARGE":
                            _logger.LogWarning("User {UserId} uploaded a file with dimensions larger than allowed.", userId);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "The uploaded file dimensions exceed the allowed limit (500x500 px).");


                        case "FILE_IS_TOO_SMALL":
                            _logger.LogWarning("User {UserId} uploaded a file that is too small.", userId);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "The uploaded file is too small.");

                        default:
                            _logger.LogWarning("User {UserId} upload failed due to validation error: {ErrorCode}", userId, error.Code);
                            return Problem(statusCode: 400, title: "Bad Request", detail: "Invalid file upload.");
                    }
                }
                else if (error.ErrorType == ErrorType.NotFound)
                {
                    _logger.LogWarning("User {UserId} not found during avatar change.", userId);
                    return Problem(statusCode: 404, title: "Not Found", detail: "The user was not found.");
                }
                else if (error.ErrorType == ErrorType.Conflict)
                {
                    _logger.LogWarning("User {UserId} does not have an avatar set.", userId);
                    return Problem(
                        statusCode: 409,
                        title: "Conflict",
                        detail: "User does not have an avatar set."
                    );
                }
                else if (error.ErrorType == ErrorType.Unknown && error.Code == "AVATAR_DELETE_FAILED")
                {
                    _logger.LogError("ChangeAvatarAsync failed: unable to delete existing avatar for user {UserId}.", userId);
                    return Problem(
                        statusCode: 500,
                        title: "Internal Server Error",
                        detail: "Failed to delete the existing avatar file."
                    );
                }
                else if (error.ErrorType == ErrorType.Unknown && error.Code == "AVATAR_UPLOAD_FAILED")
                {
                    _logger.LogError("[ChangeAvatarAsync] failed: unable to upload new avatar for user {UserId}.", userId);
                    return Problem(
                        statusCode: 500,
                        title: "Internal Server Error",
                        detail: "Failed to upload avatar file."
                    );
                }
                else if (error.ErrorType == ErrorType.Unknown && error.Code == "USER_UPDATE_FAILED")
                {
                    _logger.LogError("[ChangeAvatarAsync] Failed to update user {UserId} after avatar upload.", userId);
                    return Problem(
                        statusCode: 500,
                        title: "Internal Server Error",
                        detail: "Failed to update user avatar URL in database."
                    );
                }
                ;

                _logger.LogError("Unexpected error during avatar change for user {UserId}: {ErrorCode}", userId, error.Code);
                return Problem(statusCode: 500, title: "Unexpected error", detail: "An unexpected error occurred. Please try again later or contact support.");
            }

            _logger.LogError("Unexpected null error object during avatar change for user {UserId}", userId);
            return Problem(statusCode: 500, title: "Unexpected server error", detail: "An unexpected error occurred during avatar change.");
        }

    }
}