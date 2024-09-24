using Api.Models;
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
        public async Task<IActionResult> GetUserByIdAsyn([FromRoute] string id)
        {
            if(string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(); //todo
            }

            if(id.Length != 36)
            {
                return BadRequest(); //todo
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound(); //todo
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
                return NotFound(); //todo
            }
        }
    }
}
