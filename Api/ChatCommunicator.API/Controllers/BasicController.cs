using ChatCommunicator.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatCommunicator.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasicController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public BasicController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test2()
        {

            var result = await _emailService.SendAsync("radoslaw.smo@gmail.com", "test", "test");

            return Ok("Test endpoint response");
        }
    }
}
