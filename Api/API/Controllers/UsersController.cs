using API.Contracts.Users.ChangePassword;
using API.Contracts.Users.ChangeUsername;
using API.Contracts.Users.GetChats;
using API.Contracts.Users.GetUsers;
using API.Contracts.Users.Register;
using API.Contracts.Users.UploadAvatar;
using API.DTOs;
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
    public class UsersController : BaseController
    {
        private readonly ISender _sender;
        private readonly ILogger<UsersController> _logger;
        private readonly IMapper _mapper;

        public UsersController(ISender sender, ILogger<UsersController> logger, IMapper mapper)
        {
            _sender = sender;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost()] // refactor: docs
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

        [Authorize]
        [HttpPatch("{userId}/username")] // refactor: docs
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

        [Authorize]
        [HttpPatch("{userId}/password")] // refactor: docs
        public async Task<IActionResult> ChangePasswordAsync([FromRoute] Guid userId, [FromBody] ChangePasswordRequest req)
        {
            var command = new ChangePasswordCommand(userId, req.OldPassword, req.NewPassword);
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
        public async Task<IActionResult> UploadAvatarAsync([FromRoute] Guid userId, [FromForm] UploadAvatarRequest req)
        {
            var command = new UploadAvatarCommand(userId, req.File);
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
        public async Task<IActionResult> ChangeAvatarAsync([FromRoute] Guid userId, [FromForm] UploadAvatarRequest req)
        {
            var command = new ChangeAvatarCommand(userId, req.File);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "ChangeAvatarAsync - ChangeUsernameAsync", _logger);
        }

        [Authorize]
        [HttpGet("{userId}/friend-invitations")] // refactor: docs, command to query, blad cqrs
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

        [Authorize]
        [HttpGet("")]
        public async Task<IActionResult> GetUsersAsync([FromQuery] string search, [FromQuery] Guid? invitableFor)
        {
            var query = new GetUsersQuery(search, invitableFor);
            var result = await _sender.Send(query);

            if (result.IsSuccess)
            {
                return Ok(_mapper.Map<List<GetUsersResponse>>(result.Value));
            }

            return HandleError(result, "UsersController - GetUsersAsync", _logger);
        }

        [Authorize]
        [HttpGet("{userId}/chats")]
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