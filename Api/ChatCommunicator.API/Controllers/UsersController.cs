using ChatCommunicator.Contracts;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatCommunicator.Application.Controllers
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

        /// <summary>
        /// Get User By Id
        /// </summary>
        /// <remarks>
        /// Requires authorization. This endpoint returns a simplified user DTO corresponding to the given GUID.
        /// </remarks>
        /// <param name="id">The unique GUID identifier of the user.</param>
        /// <returns>A <see cref="SimpleUserDto"/> if found; otherwise, a problem detail response.</returns>
        /// <response code="200">Returns the requested user.</response>
        /// <response code="500">An unexpected error occurred.</response>
        /// <example>
        /// GET /api/users/get-user-by-id/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer {token}
        /// </example>
        [Authorize]
        [HttpGet("get-user-by-id/{id}")]
        [ProducesResponseType(typeof(SimpleUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Get Users By Text
        /// </summary>
        /// <remarks>
        /// Optionally excludes the currently authenticated user from the results.
        /// </remarks>
        /// <param name="text">The substring to search for in usernames.</param>
        /// <param name="excludeCurrentUser">Indicates whether to exclude the current user from the results.</param>
        /// <returns>A list of matching <see cref="SimpleUserDto"/> or a problem detail response.</returns>
        /// <response code="200">List of matching users retrieved.</response>
        /// <response code="400">The search term is invalid (too short or too long).</response>
        /// <response code="401">Authorization token is invalid or missing.</response>
        /// <response code="500">An unexpected error occurred.</response>
        /// <example>
        /// GET /api/users/get-users-by-text/john?excludeCurrentUser=true
        /// Authorization: Bearer {token}
        /// </example>
        [Authorize]
        [HttpGet("get-users-by-text/{text}")]
        [ProducesResponseType(typeof(List<SimpleUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersByTextAsync([FromRoute] string text, bool excludeCurrentUser = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Problem(
                    statusCode: 400,
                    title: "Bad Request",
                    detail: "Input value is empty."
                );
            }

            if (text.Length < 3 || text.Length > 25)
            {
                return Problem(
                    statusCode: 400,
                    title: "Bad Request",
                    detail: "Username must be between 3 and 25 characters long."
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
