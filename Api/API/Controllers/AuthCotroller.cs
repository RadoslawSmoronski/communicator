using Application;
using Application.Auth.Commands.LoginUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Result;
using System.Drawing;

namespace API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly ISender _sender;

        public AuthController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("login")]
        public async Task<ActionResult<Guid>> LoginAsync()
        {
            var command = new LoginUserCommand("testuser1@mail.com", "testUser1Password123$");
            var result = await _sender.Send(command);
            var error = result.Error;

            if (result.IsSuccess)
            {
                return result.Value;
            }
            else if (error is not null)
            {
                return error.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(),
                    _ => BadRequest()
                };
            }


            return BadRequest();
        }
    }
}
