using Api.Models;
using Api.Models.Dtos.Controllers.UsersController;
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

                var responseOk = _responseHttpFactory.Create<UsersDto>
                               (ResponseHttpType.Success, "User found.", _mapper.Map<UsersDto>(user));

                return Ok(responseOk);
            }
            catch (Exception ex)
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.InternalServerError, "An internal server error occurred.");

                return StatusCode(500, response);
            }
        }

        [HttpGet("getUsersByText/{text}")]
        //[Authorize]
        public async Task<IActionResult> GetUsersByTextAsync([FromRoute] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.BadRequest,
                               "Input value is empty.");

                return BadRequest(response);
            }

            if (text.Length > 25 || text.Length < 3)
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.BadRequest,
                               "Username must be more than 3 characters and less than 25.");

                return BadRequest(response);
            }

            try
            {
                var users = await _userManager.Users.Where(x => x.UserName!.Contains(text))
                .ToListAsync();

                if (users == null || users.Count == 0)
                {
                    var response = _responseHttpFactory.Create
                                   (ResponseHttpType.NotFound,
                                   "There is no user with this username.");

                    return NotFound(response);
                }

                var userDtos = _mapper.Map<List<UsersDto>>(users);

                var responseOk = _responseHttpFactory.Create<List<UsersDto>>
                              (ResponseHttpType.Success,
                              "User/s found.",
                              userDtos);

                return Ok(responseOk);
            }
            catch (Exception ex)
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.InternalServerError, "An internal server error occurred.");

                return StatusCode(500, response);
            }
        }
    }
}
