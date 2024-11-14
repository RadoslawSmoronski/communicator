using Api.Models;
using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
using Api.Models.Dtos.Controllers.UsersController;
using Api.Models.Dtos.Controllers.UsersController.GetUser;
using Api.Models.Dtos.Controllers.UsersController.GetUsers;
using Api.Models.Dtos.Responses;
using Api.Models.Dtos.Responses.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
        private readonly ResponseHttpFactory _responseHttpFactory;

        public UsersController(UserManager<UserAccount> userManager, IMapper mapper,
            ResponseHttpFactory responseHttpFactory)
        {
            _userManager = userManager;
            _mapper = mapper;
            _responseHttpFactory = responseHttpFactory;
        }

        [HttpGet("getUserById/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] string id)
        {

            if (string.IsNullOrWhiteSpace(id))
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.BadRequest, "Id is required.");

                return BadRequest(response);
            }

            if (!Guid.TryParse(id, out Guid result))
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.BadRequest, "Not valid format.");

                return BadRequest(response);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    var response = _responseHttpFactory.Create
                                   (ResponseHttpType.NotFound, "User does not exist.");

                    return NotFound(response);
                }

                var responseOk = _responseHttpFactory.Create<UserAccount>
                               (ResponseHttpType.Success, $"User with id {id} successfully found", user);

                return Ok(responseOk);
            }
            catch (Exception ex)
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.InternalServerError, "An internal server error occurred.");

                return StatusCode(500, response);
            }
        }

        //[HttpGet("getUsersByText/{text}")]
        //[Authorize]
        //public async Task<IActionResult> getUsersByTextAsync([FromRoute] string text)
        //{
        //    if (string.IsNullOrWhiteSpace(text))
        //    {
        //        return BadRequest(new GetUsersResponseFailedDto()
        //        {
        //            Succeeded = false,
        //            Message = "Input value is empty."
        //        });
        //    }
            
        //    if(text.Length > 25 || text.Length < 3)
        //    {
        //        return BadRequest(new GetUsersResponseFailedDto()
        //        {
        //            Succeeded = false,
        //            Message = "Username must be more than 3 characters and less than 25."
        //        });
        //    }

        //    try
        //    {
        //        var users = await _userManager.Users
        //            .Where(x => x.UserName!.Contains(text))
        //            .ToListAsync();

        //        if (users == null || users.Count == 0)
        //        {
        //            return NotFound(new GetUsersResponseFailedDto()
        //            {
        //                Succeeded = false,
        //                Message = "There is no user with this username."
        //            });
        //        }

        //        var userDtos = _mapper.Map<List<GetUsersUserResponseDto>>(users);

        //        return Ok(new GetUsersResponseOkDto()
        //        {
        //            Succeeded = true,
        //            Message = "Users were found successfully.",
        //            Users = userDtos
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new GetUsersResponseFailedDto
        //        {
        //            Succeeded = false,
        //            Message = "An internal server error occurred."
        //        });
        //    }
        //}
    }
}
