using API.Contracts.Auth.ConfirmEmail;
using API.Contracts.Auth.Login;
using API.Contracts.Auth.RefreshAccessToken;
using API.Contracts.Auth.RequestPasswordReset;
using API.Contracts.ResetPassword;
using API.DTOs;
using Application.Auth.Commands.ConfirmEmail;
using Application.Auth.Commands.LoginUser;
using Application.Auth.Commands.RefreshAccessToken;
using Application.Auth.Commands.RequestPasswordReset;
using Application.Auth.Commands.ResetPassword;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly ISender _sender;
        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;

        public AuthController(ISender sender, ILogger<AuthController> logger, IMapper mapper)
        {
            _sender = sender;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost("login")] // refactor: docs
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest req)
        {
            _logger.LogInformation("[AuthController - LoginAsync] Login attempt for email: {Email}", req.Email);

            var command = new LoginUserCommand(req.Email, req.Password);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[AuthController - LoginAsync] Login successful for email: {Email}", req.Email);
                return Ok(_mapper.Map<LoginResponse>(result.Value));
            }

            return HandleError(result, "AuthController - LoginAsync", _logger);
        }

        [HttpPost("refresh-token")] // refactor: docs
        public async Task<IActionResult> RefreshAccessTokenAsync([FromBody] RefreshAccessTokenRequest req)
        {
            _logger.LogInformation("[AuthController - RefreshAccessTokenAsync] Refresh token attempt: {RefreshToken}", req.RefreshToken);

            var command = new RefreshAccessTokenCommand(req.RefreshToken);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[AuthController - RefreshAccessTokenAsync] Refresh token successful for: {RefreshToken}", req.RefreshToken);
                return Ok(_mapper.Map<RefreshAccessTokenResponse>(result.Value));
            }

            return HandleError(result, "AuthController - RefreshAccessTokenAsync", _logger);
        }

        [HttpPost("confirm-email")] // refactor: docs, test after register endpoint will have done
        public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest req)
        {
            var command = new ConfirmEmailCommand(req.UserId, WebUtility.UrlDecode(req.ConfirmationToken));
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "AuthController - RefreshAccessTokenAsync", _logger);
        }

        [HttpPost("request-password-reset")] // refactor: docs
        public async Task<IActionResult> RequestPasswordResetAsync([FromBody] RequestPasswordResetRequest req)
        {
            _logger.LogInformation("[AuthController - RequestPasswordResetAsync] Password reset requested for email: {Email}", req.Email);

            var command = new RequestPasswordResetCommand(req.Email);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("[AuthController - RequestPasswordResetAsync] Password reset request successful for email: {Email}", req.Email);
                return Ok(result.Value); // refactor: delete if not development mode
            }

            _logger.LogWarning("[AuthController - RequestPasswordResetAsync] Password reset request failed for email: {Email}. Error: {Error}", req.Email, result.Error?.Description);
            return HandleError(result, "AuthController - RequestPasswordResetAsync", _logger);
        }

        [HttpPost("password-reset")] // refactor: docs
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest req)
        {
            var command = new ResetPasswordCommand(req.UserId, req.CodedToken, req.NewPassword);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value); // refactor: delete if not development mode
            }

            return HandleError(result, "ResetPasswordAsync - RequestPasswordResetAsync", _logger);
        }
    }
}
