using ChatCommunicator.API.Controllers;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.FriendsController;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;

namespace ChatCommunicator.Application.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly ITokenService _tokenManager;
        private readonly IFriendsService _friendsService;
        private readonly IAccountManager _accountManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ITokenService tokenManager,
            IFriendsService friendsService,
            IAccountManager accountManager,
            ILogger<UsersController> logger)
        {
            _tokenManager = tokenManager;
            _friendsService = friendsService;
            _accountManager = accountManager;
            _logger = logger;
        }

        /// <summary>
        /// Register user
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
        /// POST /api/users
        /// {
        ///     "email": "email@email.com",
        ///     "username": "usernameTest",
        ///     "password": "StrongPassword123!"
        /// }
        /// </code>
        /// </example>
        [HttpPost()]
        [ProducesResponseType(typeof(RegisteredDto), StatusCodes.Status201Created)]
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
        /// Change Username
        /// </summary>
        /// <remarks>
        /// Authenticated users can change their username. The new username must be unique.
        /// </remarks>
        /// <param name="userId">The ID of the user whose username is to be changed (from query).</param>
        /// <param name="newUsername">The new username to assign (from body).</param>
        /// <returns>The new username or a detailed error response.</returns>
        /// <response code="200">Username successfully updated.</response>
        /// <response code="400">Invalid or missing username.</response>
        /// <response code="401">User is not authenticated or token is invalid.</response>
        /// <response code="409">The username is already taken.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// PATCH /api/users/{userId}/username?userId=123e4567-e89b-12d3-a456-426614174000
        /// Authorization: Bearer {token}
        /// "new_name_123"
        /// </code>
        /// </example>
        [Authorize]
        [HttpPatch("{userId}/username")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeUsernameAsync([FromRoute] Guid userId, [FromBody] string newUsername)
        {
            var validate = ValidateAndGetUserId("ChangeUsernameAsync", _logger, out Guid loggedUserId);

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
        /// <param name="userId">The ID of the user whose password is to be changed (from query).</param>
        /// <param name="changePasswordDto">The DTO containing the old and new passwords.</param>
        /// <returns>A success response or a detailed error response.</returns>
        /// <response code="200">Password successfully changed.</response>
        /// <response code="400">Invalid input (e.g., missing user ID, invalid old or new password).</response>
        /// <response code="401">User is not authenticated, token is invalid, or old password is incorrect.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// PATCH /api/users/{userId}/password?userId=123e4567-e89b-12d3-a456-426614174000
        /// Authorization: Bearer {token}
        /// {
        ///     "oldPassword": "OldPassword123!",
        ///     "newPassword": "NewPassword456!"
        /// }
        /// </code>
        /// </example>
        [Authorize]
        [HttpPatch("{userId}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePasswordAsync([FromRoute] Guid userId, [FromBody] ChangePasswordDto changePasswordDto)
        {
            var validate = ValidateAndGetUserId("ChangePasswordAsync", _logger, out Guid loggerUserId);

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
            else if (result.Error != null && result.Error.Code == "OLDPASSWORD_IS_INCORRECT")
            {
                _logger.LogWarning("[ChangePasswordAsync] Old password is incorrect for user {UserId}.", userId);
                        return Problem(
                        statusCode: 401,
                        title: "OLDPASSWORD_IS_INCORRECT",
                        detail: "Old password is incorrect for user " + userId
                    );
            }


            return HandleError(result, "ChangePasswordAsync", _logger);
        }

        /// <summary>
        /// Upload Avatar
        /// </summary>
        /// <remarks>
        /// Authenticated users can upload a new avatar image.<br/>
        /// The uploaded file must meet the following requirements:
        /// <ul>
        /// <li>Maximum file size: 5 MB</li>
        /// <li>Maximum dimensions: 500x500 pixels</li>
        /// <li>Supported formats: JPEG, PNG, etc.</li>
        /// </ul>
        /// <br/>
        /// <b>Note:</b> The <c>userId</c> parameter is required and should be provided as a query parameter (e.g., <c>?userId=...</c>).
        /// </remarks>
        /// <param name="userId">The ID of the user uploading the avatar (from query).</param>
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
        /// POST /api/users/{userId}/avatar?userId=123e4567-e89b-12d3-a456-426614174000
        /// Authorization: Bearer {token}
        /// Content-Type: multipart/form-data
        ///
        /// Form Data:
        /// file: avatar_image.png
        /// </code>
        /// </example>
        [Authorize]
        [HttpPost("{userId}/avatar")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadAvatarAsync([FromRoute] Guid userId, [FromForm] UploadAvatarDto uploadAvatarDto)
        {
            var validate = ValidateAndGetUserId("UploadAvatarAsync", _logger, out Guid loggerUserId);

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
        /// Authenticated users can delete their avatar.<br/>
        /// This action removes the avatar associated with the user account.
        /// <br/><br/>
        /// <b>Note:</b> The <c>userId</c> parameter is required and should be provided as a query parameter (e.g., <c>?userId=...</c>).
        /// </remarks>
        /// <param name="userId">The ID of the user whose avatar is to be deleted (from query).</param>
        /// <returns>A success response or a detailed error response.</returns>
        /// <response code="200">Avatar successfully deleted.</response>
        /// <response code="400">Invalid or missing user ID.</response>
        /// <response code="401">User is not authenticated or token is invalid.</response>
        /// <response code="404">User not found in the system.</response>
        /// <response code="409">No avatar is set for this user, so there is nothing to delete.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        /// <example>
        /// <code>
        /// DELETE /api/users/{userId}/avatar?userId=123e4567-e89b-12d3-a456-426614174000
        /// Authorization: Bearer {token}
        /// </code>
        /// </example>
        [Authorize]
        [HttpDelete("{userId}/avatar")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAvatarAsync([FromRoute] Guid userId)
        {
            var validate = ValidateAndGetUserId("DeleteAvatarAsync", _logger, out Guid loggedUserId);

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
        /// Authenticated users can change their avatar by uploading a new image.<br/>
        /// The uploaded file must meet the following requirements:
        /// <ul>
        /// <li>Maximum file size: 5 MB</li>
        /// <li>Maximum dimensions: 500x500 pixels</li>
        /// <li>Supported formats: JPEG, PNG, etc.</li>
        /// </ul>
        /// <br/>
        /// <b>Note:</b> The <c>userId</c> parameter is required and should be provided as a query parameter (e.g., <c>?userId=...</c>).
        /// </remarks>
        /// <param name="userId">The ID of the user whose avatar is to be changed (from query).</param>
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
        /// PUT /api/users/{userId}/avatar?userId=123e4567-e89b-12d3-a456-426614174000
        /// Authorization: Bearer {token}
        /// Content-Type: multipart/form-data
        ///
        /// Form Data:
        /// file: new_avatar_image.png
        /// </code>
        /// </example>
        [Authorize]
        [HttpPut("{userId}/avatar")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangeAvatarAsync([FromRoute] Guid userId, [FromForm] UploadAvatarDto uploadAvatarDto)
        {
            var validate = ValidateAndGetUserId("ChangeAvatarAsync", _logger, out Guid loggedUserId);

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

        /// <summary>
        /// Get Friend Invitations
        /// </summary>
        /// <remarks>
        /// Returns all pending friend invitations for the specified user.
        /// If the user does not exist or the input is invalid, appropriate error responses are returned.
        /// </remarks>
        /// <param name="userId">The GUID of the user whose invitations are to be retrieved (from query).</param>
        /// <returns>
        /// A list of users who sent invitations, or an error response.
        /// </returns>
        /// <response code="200">Invitations retrieved successfully.</response>
        /// <response code="400">Invalid user ID (e.g., malformed GUID).</response>
        /// <response code="401">Unauthorized - JWT token required.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">Unexpected server error.</response>
        /// <example>
        /// <code>
        /// GET /api/users/{userId}/friend-invitations?userId=3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer {token}
        /// </code>
        /// </example>
        [Authorize]
        [HttpGet("{userId}/friend-invitations")]
        [ProducesResponseType(typeof(List<SimpleUserWithAvatarDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFriendInvitationsAsync([FromRoute] Guid userId)
        {
            var validate = ValidateAndGetUserId("GetFriendInvitationsAsync", _logger, out Guid loggedUserId);

            var result = await _friendsService.GetInvitationsAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[GetFriendInvitationsAsync] Successfully retrieved invitations for UserId: {UserId}. Count: {Count}",
                    userId, result.Value?.Count() ?? 0);
                return Ok(result.Value);
            }

            return HandleError(result, "GetFriendInvitationsAsync", _logger);
        }

        /// <summary>
        /// Get Users
        /// </summary>
        /// <remarks>
        /// Authenticated users can search for other users by username or display name.
        /// Results exclude users who are already friends with, or already invited by, the specified user.
        /// Requires a valid JWT token.
        /// </remarks>
        /// <param name="search">Optional text to search users by (e.g., username or display name).</param>
        /// <param name="invitableFor">The GUID of the user for whom to find invitable users (required).</param>
        /// <returns>
        /// A list of users matching the search criteria and available for invitation. Can be empty.
        /// </returns>
        /// <response code="200">Users retrieved successfully.</response>
        /// <response code="400">Invalid request (e.g., missing or malformed 'invitableFor' GUID).</response>
        /// <response code="401">Unauthorized - JWT token missing or invalid.</response>
        /// <response code="500">Unexpected server error.</response>
        /// <example>
        /// <code>
        /// GET /api/users?search=john&amp;invitableFor=3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer {token}
        /// </code>
        /// </example>
        [Authorize]
        [HttpGet("")]
        [ProducesResponseType(typeof(List<UserToInviteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsersAsync([FromQuery] string search, [FromQuery] Guid invitableFor)
        {
            //REFACTOR
            if (invitableFor == Guid.Empty)
            {
                return Problem(
                    statusCode: 400,
                    title: "Bad Request",
                    detail: "The 'invitableFor' query parameter is required and must be a valid GUID. Other search functionality is currently not supported."
                );
            }
            //REFACTOR

            var validate = ValidateAndGetUserId("GetUsersAsync", _logger, out Guid userId);

            if (validate != null)
            {
                return validate;
            }

            var result = await _friendsService.GetUsersToInviteByTextAsync(invitableFor, search); //REFACTOR

            if (result.IsSuccess)
            {
                _logger.LogInformation("[GetUsersAsync] Successfully retrieved users to invite for UserId: {UserId}, SearchText: {Text}. Count: {Count}",
                    userId, search, result.Value?.Count() ?? 0);
                return Ok(result.Value);
            }

            return HandleError(result, "GetUsersToInviteByTextAsync", _logger);
        }

        /// <summary>
        /// Get Friends
        /// </summary>
        /// <remarks>
        /// Fetches all users marked as friends of the specified user GUID.
        /// Returns errors for invalid input or unexpected issues.
        /// </remarks>
        /// <param name="userId">The GUID of the user whose friends are to be retrieved (from query).</param>
        /// <returns>
        /// List of friends or an error response.
        /// </returns>
        /// <response code="200">Friends list retrieved successfully.</response>
        /// <response code="400">Invalid user ID.</response>
        /// <response code="401">Unauthorized - JWT token required.</response>
        /// <response code="500">Unexpected server error.</response>
        /// <example>
        /// <code>
        /// GET /api/users/{userId}/friends?userId=3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer {token}
        /// </code>
        /// </example>
        [Authorize]
        [HttpGet("{userId}/friends")]
        [ProducesResponseType(typeof(List<SimpleUserWithAvatarDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFriendsAsync([FromRoute] Guid userId)
        {
            var result = await _friendsService.GetFriendsAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[GetFriendsAsync] Friends list fetched successfully. UserId: {UserId}", userId);
                return Ok(result.Value);
            }

            return HandleError(result, "GetFriendsAsync", _logger);
        }

    }
}