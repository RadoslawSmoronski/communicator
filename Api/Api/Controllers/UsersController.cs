using Api.Models;
using Api.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersController(UserManager<UserAccount> userManager, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("getUserById/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] Guid id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());

                return Ok(_mapper.Map<SimpleUserDto>(user));
            }
            catch (Exception)
            {
                return Problem(
                statusCode: 500,
                title: "Unexpected server error",
                detail: "An unexpected error occurred while retrieving the user."
                );
            }
        }

        [HttpGet("getUsersByText/{text}")]
        [Authorize]
        public async Task<IActionResult> GetUsersByTextAsync([FromRoute] string text, bool excludeCurrentUser = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Problem(
                    detail: "Input value is empty.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            if (text.Length > 25 || text.Length < 3)
            {
                return Problem(
                    detail: "Username must be between 3 and 25 characters long.",
                    statusCode: 400,
                    title: "Bad Request"
                );
            }

            try
            {
                var usersQuery = _userManager.Users.Where(x => x.UserName!.Contains(text));

                if (excludeCurrentUser)
                {
                    var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!Guid.TryParse(userIdClaim, out var userId))
                    {
                        return Problem(
                            statusCode: 401,
                            title: "Unauthorized",
                            detail: "Invalid user ID in access token."
                        );
                    }

                    if (userId != Guid.Empty)
                        usersQuery = usersQuery.Where(x => x.Id != userId);
                }

                var users = await usersQuery.ToListAsync();

                return Ok(_mapper.Map<List<SimpleUserDto>>(users));
            }
            catch (Exception)
            {
                return Problem(
                statusCode: 500,
                title: "Unexpected server error",
                detail: "An unexpected error occurred while retrieving the users."
                );
            }
        }
    }
}
