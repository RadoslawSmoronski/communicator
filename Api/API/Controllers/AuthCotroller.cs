using API.DTOs;
using Application.Auth.Commands.ConfirmEmail;
using Application.Auth.Commands.LoginUser;
using Application.Auth.Commands.RefreshAccessToken;
using Application.Auth.Commands.RequestPasswordReset;
using Application.Auth.Commands.ResetPassword;
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

        [HttpPost("request-password-reset")] // refactor: docs
        public async Task<IActionResult> RequestPasswordResetAsync([FromBody] RequestPasswordResetDto requestPasswordResetDto)
        {
            _logger.LogInformation("[AuthController - RequestPasswordResetAsync] Password reset requested for email: {Email}", requestPasswordResetDto.Email);

            var command = new RequestPasswordResetCommand(requestPasswordResetDto.Email);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[AuthController - RequestPasswordResetAsync] Password reset request successful for email: {Email}", requestPasswordResetDto.Email);
                return Ok(result.Value);
            }

            _logger.LogWarning("[AuthController - RequestPasswordResetAsync] Password reset request failed for email: {Email}. Error: {Error}", requestPasswordResetDto.Email, result.Error?.Description);
            return HandleError(result, "AuthController - RequestPasswordResetAsync", _logger);
        }

        [HttpPost("password-reset")] // refactor: docs
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto resetPasswordResetDto)
        {
            var command = new ResetPasswordCommand(resetPasswordResetDto.UserId, resetPasswordResetDto.CodedToken, resetPasswordResetDto.NewPassword);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleError(result, "ResetPasswordAsync - RequestPasswordResetAsync", _logger);
        }
    }
}
