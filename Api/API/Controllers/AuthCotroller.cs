using API.DTOs;
using Application.Auth.Commands.LoginUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Result;

namespace API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly ISender _sender;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ISender sender, ILogger<AuthController> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto loginDto)
        {
            _logger.LogInformation("[AuthController - LoginAsync] Login attempt for email: {Email}", loginDto.Email);

            var command = new LoginUserCommand(loginDto.Email, loginDto.Password);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[AuthController - LoginAsync] Login successful for email: {Email}", loginDto.Email);
                return Ok(result.Value);
            }

            return HandleError(result, "AuthController - LoginAsync", _logger);
        }
    }
}
