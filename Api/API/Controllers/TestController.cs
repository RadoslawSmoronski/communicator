using API.DTOs;
using Application.Repositories;
using Application.Users.Commands.RegisterUser;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public TestController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet()]
        public async Task<IActionResult> TestEndpointAsync()
        {
            return Ok();
        }
    }
}
