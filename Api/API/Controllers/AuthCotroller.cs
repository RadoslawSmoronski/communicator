using API.DTOs;
using Application.Auth.Commands.ConfirmEmail;
using Application.Auth.Commands.LoginUser;
using Application.Auth.Commands.RefreshAccessToken;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Result;
using System.Net;

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

        [HttpPost("login")] // refactor: docs
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

        [HttpPost("refresh-token")] // refactor: docs
        public async Task<IActionResult> RefreshAccessTokenAsync([FromBody] RefreshAccessTokenDto refreshAccessTokenDto)
        {
            _logger.LogInformation("[AuthController - RefreshAccessTokenAsync] Refresh token attempt: {RefreshToken}", refreshAccessTokenDto.RefreshToken);

            var command = new RefreshAccessTokenCommand(refreshAccessTokenDto.RefreshToken);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[AuthController - RefreshAccessTokenAsync] Refresh token successful for: {RefreshToken}", refreshAccessTokenDto.RefreshToken);
                return Ok(result.Value);
            }

            return HandleError(result, "AuthController - RefreshAccessTokenAsync", _logger);
        }

        [HttpPost("confirm-email")] // refactor: docs, test after register endpoint will have done
        public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailDto confirmEmailDto)
        {
            var command = new ConfirmEmailCommand(confirmEmailDto.UserId, WebUtility.UrlDecode(confirmEmailDto.ConfirmationToken));
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "AuthController - RefreshAccessTokenAsync", _logger);
        }
    }
}
