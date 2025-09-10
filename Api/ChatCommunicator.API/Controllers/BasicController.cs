using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatCommunicator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasicController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IAuthService _authService;

        public BasicController(IEmailService emailService, IAuthService authService)
        {
            _emailService = emailService;
            _authService = authService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test2()
        {

            var result = await _emailService.SendAsync("radoslaw.smo@gmail.com", "test", "test");

            return Ok("Test endpoint response");
        }

        [HttpGet("test3")]
        public async Task<IActionResult> Test3()
        {

            var result = await _authService.SendPasswordResetEmailAsync("radoslaw.smo@gmail.com");

            return Ok("Test endpoint response");
        }
    }
}
