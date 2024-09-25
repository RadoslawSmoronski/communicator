using Api.Models;
using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
using Api.Models.Dtos.Controllers.UsersController;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;

        public UsersController(UserManager<UserAccount> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("getUserById/{id}")]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] string id)
        {
            if(string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new GetUserResponseFailedDto()
                {
                    Succeeded = false,
                    Message = "Input value is empty."
                });
            }

            if(id.Length != 36)
            {
                return BadRequest(new GetUserResponseFailedDto()
                {
                    Succeeded = false,
                    Message = "User id contain 36 characters."
                });
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound(new GetUserResponseFailedDto()
                    {
                        Succeeded = false,
                        Message = "User does not exist."
                    });
                }

                return Ok(new GetUserResponseOkDto()
                {
                    Succeeded = true,
                    Message = $"User with id {id} successfully found",
                    User = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GetUserResponseFailedDto
                {
                    Succeeded = false,
                    Message = "An internal server error occurred."
                });
            }
        }
    }
}
