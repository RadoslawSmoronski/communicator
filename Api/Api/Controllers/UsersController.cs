using Api.Models;
using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
using Api.Models.Dtos.Controllers.UsersController;
using Api.Models.Dtos.Controllers.UsersController.GetUser;
using Api.Models.Dtos.Controllers.UsersController.GetUsers;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;

        public UsersController(UserManager<UserAccount> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
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

        [HttpGet("getUsersByText/{text}")]
        public async Task<IActionResult> getUsersByTextAsync([FromRoute] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return BadRequest(new GetUsersResponseFailedDto()
                {
                    Succeeded = false,
                    Message = "Input value is empty."
                });
            }
            
            if(text.Length > 25 || text.Length < 3)
            {
                return BadRequest(new GetUsersResponseFailedDto()
                {
                    Succeeded = false,
                    Message = "Username must be more than 3 characters and less than 25."
                });
            }

            try
            {
                var users = await _userManager.Users
                    .Where(x => x.UserName!.Contains(text))
                    .ToListAsync();

                if (users == null || users.Count == 0)
                {
                    return NotFound(new GetUsersResponseFailedDto()
                    {
                        Succeeded = false,
                        Message = "There is no user with this username."
                    });
                }

                var userDtos = _mapper.Map<List<GetUsersUserResponseDto>>(users);

                return Ok(new GetUsersResponseOkDto()
                {
                    Succeeded = true,
                    Message = "Users were found successfully.",
                    Users = userDtos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new GetUsersResponseFailedDto
                {
                    Succeeded = false,
                    Message = "An internal server error occurred."
                });
            }
        }
    }
}
