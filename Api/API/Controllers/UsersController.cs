using API.Contracts.Users.ChangePassword;
using API.Contracts.Users.ChangeUsername;
using API.Contracts.Users.GetChats;
using API.Contracts.Users.GetFriendInvitations;
using API.Contracts.Users.GetUsers;
using API.Contracts.Users.Register;
using API.Contracts.Users.UploadAvatar;
using Application.Users.Commands.ChangeAvatar;
using Application.Users.Commands.ChangePassword;
using Application.Users.Commands.ChangeUsername;
using Application.Users.Commands.DeleteAvatar;
using Application.Users.Commands.RegisterUser;
using Application.Users.Commands.UploadAvatar;
using Application.Users.Queries.GetChats;
using Application.Users.Queries.GetInvitations;
using Application.Users.Queries.GetUsers;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController(
        ISender sender,
        ILogger<UsersController> logger,
        IMapper mapper)
        : BaseController(mapper, sender)
    {
        private readonly ILogger<UsersController> _logger = logger;

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="req">The registration payload containing email, username, and password.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="RegisterResponse"/> when registration succeeds; otherwise an error response.
        /// </returns>
        /// <remarks>
        /// Route: POST api/users
        /// Authentication: Not required.
        /// </remarks>
        /// <response code="200">Registration succeeded and returns the created user.</response>
        /// <response code="400">The request payload failed validation.</response>
        /// <response code="409">A user with the same email or username already exists.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        [HttpPost()]
        [ProducesResponseType(typeof(RegisterRequest), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest req)
        {
            var command = new RegisterUserCommand(req.Email, req.Username, req.Password);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(_mapper.Map<RegisterResponse>(result.Value));
            }

            return HandleError(result, "UsersController - RegisterAsync", _logger);
        }

        /// <summary>
        /// Change user username
        /// </summary>
        /// <param name="userId">The identifier of the user whose username will be changed.</param>
        /// <param name="req">The payload containing the new username.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="ChangeUsernameResponse"/> containing the new username when successful; otherwise an error response.
        /// </returns>
        /// <remarks>
        /// Route: PATCH api/users/{userId}/username
        /// Authorization: Required.
        /// </remarks>
        /// <response code="200">Username changed successfully and the new username is returned.</response>
        /// <response code="400">Invalid request payload or business rule violation.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Not authorized to change this user's username.</response>
        /// <response code="404">User not found.</response>
        /// <response code="409">The requested username is already taken.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        [Authorize]
        [HttpPatch("{userId}/username")]
        [ProducesResponseType(typeof(ChangeUsernameResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> ChangeUsernameAsync([FromRoute] Guid userId, [FromBody] ChangeUsernameRequest req)
        {
            var command = new ChangeUsernameCommand(userId, req.NewUsername);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new ChangeUsernameResponse(NewUsername: result.Value));
            }

            return HandleError(result, "UsersController - ChangeUsernameAsync", _logger);
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="userId">The identifier of the user whose password will be changed.</param>
        /// <param name="req">The payload containing the current password and the new password.</param>
        /// <returns>
        /// Returns 200 OK when the password is successfully changed; otherwise an error response.
        /// </returns>
        /// <remarks>
        /// Route: PATCH api/users/{userId}/password
        /// Authorization: Required.
        /// </remarks>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">Invalid request payload or password policy violation.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Not authorized to change this user's password.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        [Authorize]
        [HttpPatch("{userId}/password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> ChangePasswordAsync([FromRoute] Guid userId, [FromBody] ChangePasswordRequest req)
        {
            var command = new ChangePasswordCommand(userId, req.OldPassword, req.NewPassword);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "UsersController - ChangePasswordAsync", _logger);
        }

        /// <summary>
        /// Upload user avatar
        /// </summary>
        /// <param name="userId">Identifier of the user whose avatar will be uploaded.</param>
        /// <param name="req">Form payload containing the avatar file.</param>
        /// <returns>
        /// 200 OK when the avatar is stored successfully; otherwise an error response generated by <see cref="BaseController.HandleError"/>
        /// </returns>
        /// <remarks>
        /// Route: POST api/users/{userId}/avatar
        /// Authorization: Required.
        /// Consumes: multipart/form-data.
        /// </remarks>
        /// <response code="200">Avatar uploaded successfully.</response>
        /// <response code="400">Invalid file provided (missing, zero length, unsupported type, exceeds size limit).</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Not authorized to upload avatar for this user.</response>
        /// <response code="404">User not found.</response>
        /// <response code="415">Unsupported media type.</response>
        /// <response code="500">Unexpected server error.</response>
        [Authorize]
        [HttpPost("{userId}/avatar")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadAvatarResponse),StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> UploadAvatarAsync([FromRoute] Guid userId, [FromForm] UploadAvatarRequest req)
        {
            var command = new UploadAvatarCommand(userId, req.File);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new UploadAvatarResponse(AvatarUrl: result.Value));
            }

            return HandleError(result, "UsersController - UploadAvatarAsync", _logger);
        }

        /// <summary>
        /// Delete user avatar
        /// </summary>
        /// <param name="userId">The identifier of the user whose avatar will be deleted.</param>
        /// <returns>
        /// Returns 200 OK when the avatar is successfully deleted"/>.
        /// </returns>
        /// <remarks>
        /// Route: DELETE api/users/{userId}/avatar
        /// Authorization: Required.
        /// </remarks>
        /// <response code="200">Avatar deleted successfully.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Not authorized to delete avatar for this user.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        [Authorize]
        [HttpDelete("{userId}/avatar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> DeleteAvatarAsync([FromRoute] Guid userId)
        {
            var command = new DeleteAvatarCommand(userId);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "UsersController - DeleteAvatarAsync", _logger);
        }

        /// <summary>
        /// Change user avatar
        /// </summary>
        /// <param name="userId">Identifier of the user whose avatar will be replaced.</param>
        /// <param name="req">Form payload containing the new avatar file.</param>
        /// <returns>
        /// Returns 200 OK when the avatar is successfully replaced;
        /// </returns>
        /// <remarks>
        /// Route: PUT api/users/{userId}/avatar
        /// Authorization: Required.
        /// Consumes: multipart/form-data.
        /// </remarks>
        /// <response code="200">Avatar changed successfully.</response>
        /// <response code="400">Invalid file provided (missing, zero length, unsupported type, exceeds size limit).</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Not authorized to change avatar for this user.</response>
        /// <response code="404">User not found.</response>
        /// <response code="415">Unsupported media type.</response>
        /// <response code="500">Unexpected server error.</response>
        [Authorize]
        [HttpPut("{userId}/avatar")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(UploadAvatarResponse),StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> ChangeAvatarAsync([FromRoute] Guid userId, [FromForm] UploadAvatarRequest req)
        {
            var command = new ChangeAvatarCommand(userId, req.File);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new UploadAvatarResponse(AvatarUrl: result.Value));
            }

            return HandleError(result, "UsersController - ChangeAvatarAsync", _logger);
        }

        /// <summary>
        /// Get user's friendship invitations
        /// </summary>
        /// <param name="userId">The identifier of the user whose friendship invitations will be retrieved.</param>
        /// <returns>
        /// Returns 200 OK with a list of <see cref="GetFriendshipInvitationResponse"/> when successful;
        /// </returns>
        /// <remarks>
        /// Route: GET api/users/{userId}/friend-invitations
        /// Authorization: Required.
        /// </remarks>
        /// <response code="200">Friendship invitations retrieved successfully.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Not authorized to access this user's invitations.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        [Authorize]
        [HttpGet("{userId}/friend-invitations")]
        [ProducesResponseType(typeof(List<GetFriendshipInvitationResponse>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> GetFriendInvitationsAsync([FromRoute] Guid userId)
        {
            var command = new GetInvitationsCommand(userId);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(_mapper.Map<List<GetFriendshipInvitationResponse>>(result.Value));
            }

            return HandleError(result, "UsersController - GetFriendInvitationsAsync", _logger);
        }

        /// <summary>
        /// Get users
        /// </summary>
        /// <param name="search">
        /// The free-text search term used to match users (e.g. username or email fragments). Required; pass empty string to return no results or default set (implementation-defined).
        /// </param>
        /// <param name="canBeInvitedByUserId">
        /// Required user identifier. When provided, only users that can receive a friendship invitation from the specified user are returned (e.g. excluding already friends, pending invitations, self).
        /// </param>
        /// <returns>
        /// Returns 200 OK with a list of <see cref="GetUsersResponse"/> items when successful;"/>.
        /// </returns>
        /// <remarks>
        /// Route: GET api/users
        /// Authorization: Required.
        /// Query parameters:
        ///   - search: string
        ///   - canBeInvitedByUserId: Guid (required)
        ///
        /// Note: Currently, only retrieval using the canBeInvitedByUserId filter is supported by the application, as it is the only required scenario.
        /// Search-only queries may be ignored or have limited behavior depending on the current implementation.
        /// </remarks>
        /// <response code="200">Users retrieved successfully.</response>
        /// <response code="400">Validation or business rule failure (e.g. invalid GUID format).</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Not authorized to perform this operation.</response>
        /// <response code="500">Unexpected server error.</response>
        [Authorize]
        [HttpGet("")]
        [ProducesResponseType(typeof(List<GetUsersResponse>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> GetUsersAsync([FromQuery] string search, [FromQuery] Guid? canBeInvitedByUserId)
        {
            var query = new GetUsersQuery(search, canBeInvitedByUserId);
            var result = await _sender.Send(query);

            if (result.IsSuccess)
            {
                return Ok(_mapper.Map<List<GetUsersResponse>>(result.Value));
            }

            return HandleError(result, "UsersController - GetUsersAsync", _logger);
        }

        /// <summary>
        /// Get user's chats
        /// </summary>
        /// <param name="userId">The identifier of the user whose chats will be retrieved.</param>
        /// <param name="onlyFriends">
        /// When true, returns chats that are with friends only; when false, returns all chats available to the user.
        /// </param>
        /// <returns>
        /// Returns 200 OK with a list of <see cref="GetChatsResponse"/> when successful;
        /// </returns>
        /// <remarks>
        /// Route: GET api/users/{userId}/chats
        /// Authorization: Required.
        /// Query parameters:
        ///   - onlyFriends: bool
        /// </remarks>
        /// <response code="200">Chats retrieved successfully.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Not authorized to access this user's chats.</response>
        /// <response code="404">User not found.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        [Authorize]
        [HttpGet("{userId}/chats")]
        [ProducesResponseType(typeof(List<GetChatsResponse>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> GetChatsAsync([FromRoute] Guid userId, [FromQuery] bool onlyFriends)
        {
            var query = new GetChatsQuery(userId, onlyFriends);
            var result = await _sender.Send(query);

            if (result.IsSuccess)
            {
                return Ok(_mapper.Map<List<GetChatsResponse>>(result.Value));
            }

            return HandleError(result, "UsersController - GetChatsAsync", _logger);
        }

    }
}