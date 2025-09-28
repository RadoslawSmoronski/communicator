using API.DTOs;
using Application.Auth.Commands.LoginUser;
using Application.Users.Commands.ChangeUsername;
using Application.Users.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

    }
}
