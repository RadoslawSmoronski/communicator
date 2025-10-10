using API.DTOs;
using Application.DTOs;
using Application.FriendInvitations.GetInvitations;
using Application.Users.Commands.ChangeAvatar;
using Application.Users.Commands.ChangePassword;
using Application.Users.Commands.ChangeUsername;
using Application.Users.Commands.DeleteAvatar;
using Application.Users.Commands.RegisterUser;
using Application.Users.Commands.UploadAvatar;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : BaseController
    {
        private readonly ISender _sender;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ISender sender, ILogger<UsersController> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        [HttpPost()] // refactor: docs
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            var command = new RegisterUserCommand(registerDto.Email, registerDto.Username, registerDto.Password);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleError(result, "UsersController - RegisterAsync", _logger);
        }

        [Authorize]
        [HttpPatch("{userId}/username")] // refactor: docs
        public async Task<IActionResult> ChangeUsernameAsync([FromRoute] Guid userId, [FromBody] ChangeUsernameDto changeUsernameDto)
        {
            var command = new ChangeUsernameCommand(userId, changeUsernameDto.NewUsername);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleError(result, "UsersController - ChangeUsernameAsync", _logger);
        }

        [Authorize]
        [HttpPatch("{userId}/password")] // refactor: docs
        public async Task<IActionResult> ChangePasswordAsync([FromRoute] Guid userId, [FromBody] ChangePasswordDto changePasswordDto)
        {
            var command = new ChangePasswordCommand(userId, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "ChangePasswordAsync - ChangeUsernameAsync", _logger);
        }

        [Authorize]
        [HttpPost("{userId}/avatar")]
        [Consumes("multipart/form-data")] // refactor: docs
        public async Task<IActionResult> UploadAvatarAsync([FromRoute] Guid userId, [FromForm] UploadAvatarDto uploadAvatarDto)
        {
            var command = new UploadAvatarCommand(userId, uploadAvatarDto.File);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "ChangePasswordAsync - ChangeUsernameAsync", _logger);
        }

        [Authorize]
        [HttpDelete("{userId}/avatar")] // refactor: docs
        public async Task<IActionResult> DeleteAvatarAsync([FromRoute] Guid userId)
        {
            var command = new DeleteAvatarCommand(userId);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "DeleteAvatarAsync - ChangeUsernameAsync", _logger);
        }

        [Authorize]
        [HttpPut("{userId}/avatar")]
        [Consumes("multipart/form-data")] // refactor: docs
        public async Task<IActionResult> ChangeAvatarAsync([FromRoute] Guid userId, [FromForm] UploadAvatarDto uploadAvatarDto)
        {
            var command = new ChangeAvatarCommand(userId, uploadAvatarDto.File);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "ChangeAvatarAsync - ChangeUsernameAsync", _logger);
        }

        [Authorize]
        [HttpGet("{userId}/friend-invitations")] // refactor: docs
        public async Task<IActionResult> GetFriendInvitationsAsync([FromRoute] Guid userId)
        {
            var command = new GetInvitationsCommand(userId);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleError(result, "UsersController - GetFriendInvitationsAsync", _logger);
        }

    }
}