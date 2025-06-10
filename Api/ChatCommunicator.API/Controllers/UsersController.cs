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
        /// GetUserById.
        /// </summary>
        /// <remarks>
        /// This endpoint retrieves a user based on their unique GUID identifier.
        /// The request requires authorization, and the user must exist in the system.
        /// </remarks>
        /// <param name="id">The GUID of the user to retrieve.</param>
        /// <returns>
        /// A <see cref="SimpleUserDto"/> representing the user if found; otherwise, a <see cref="ProblemDetails"/> response.
        /// </returns>
        /// <response code="200">User successfully retrieved.</response>
        /// <response code="500">Unexpected server error occurred while retrieving the user.</response>
        /// <example>
        /// <code>
        /// GET /api/users/getUserById/3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// Authorization: Bearer {token}
        /// </code>
        /// </example>
        [Authorize]
        [HttpGet("getUserById/{id}")]
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
        /// GetUsersByText.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of users whose usernames contain the provided text.
        /// Optionally, the currently authenticated user can be excluded from the results.
        /// </remarks>
        /// <param name="text">The text to search for within usernames.</param>
        /// <param name="excludeCurrentUser">Whether to exclude the currently authenticated user from the result.</param>
        /// <returns>
        /// A list of <see cref="SimpleUserDto"/> matching the search criteria; or a <see cref="ProblemDetails"/> response on failure.
        /// </returns>
        /// <response code="200">List of matching users returned.</response>
        /// <response code="400">The search text is empty or does not meet length constraints.</response>
        /// <response code="401">Access token is missing or user ID is invalid.</response>
        /// <response code="500">Unexpected server error occurred while retrieving users.</response>
        /// <example>
        /// <code>
        /// GET /api/users/getUsersByText/john?excludeCurrentUser=true
        /// Authorization: Bearer {token}
        /// </code>
        /// </example>
        [Authorize]
        [HttpGet("getUsersByText/{text}")]
        [ProducesResponseType<LoggedUserDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
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
