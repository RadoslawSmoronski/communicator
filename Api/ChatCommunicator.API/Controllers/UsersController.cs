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
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserManager<UserAccount> userManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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
            _logger.LogInformation("[GetUserByIdAsync] Attempting to get user by id: {UserId}", id);

            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());

                if (user == null)
                {
                    _logger.LogWarning("[GetUserByIdAsync] User not found. UserId: {UserId}", id);
                    return Problem(
                        statusCode: 404,
                        title: "Not Found",
                        detail: "User not found."
                    );
                }

                _logger.LogInformation("[GetUserByIdAsync] User retrieved successfully. UserId: {UserId}", id);
                return Ok(_mapper.Map<SimpleUserDto>(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetUserByIdAsync] Unexpected error occurred while retrieving user. UserId: {UserId}", id);
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
            _logger.LogInformation("[GetUsersByTextAsync] Searching users by text: {Text}, excludeCurrentUser: {ExcludeCurrentUser}", text, excludeCurrentUser);

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("[GetUsersByTextAsync] Input text is empty.");
                return Problem(
                    statusCode: 400,
                    title: "Bad Request",
                    detail: "Input value is empty."
                );
            }

            if (text.Length < 3 || text.Length > 25)
            {
                _logger.LogWarning("[GetUsersByTextAsync] Text length out of range. Length: {Length}", text.Length);
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
                        _logger.LogWarning("[GetUsersByTextAsync] Invalid user ID in access token.");
                        return Problem(
                            statusCode: 401,
                            title: "Unauthorized",
                            detail: "Invalid user ID in access token."
                        );
                    }

                    if (userId != Guid.Empty)
                    {
                        _logger.LogInformation("[GetUsersByTextAsync] Excluding current user from results. UserId: {UserId}", userId);
                        usersQuery = usersQuery.Where(x => x.Id != userId);
                    }
                }

                var users = await usersQuery.ToListAsync();

                _logger.LogInformation("[GetUsersByTextAsync] Found {Count} users matching text: {Text}", users.Count, text);
                return Ok(_mapper.Map<List<SimpleUserDto>>(users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetUsersByTextAsync] Unexpected error occurred while retrieving users.");
                return Problem(
                    statusCode: 500,
                    title: "Unexpected server error",
                    detail: "An unexpected error occurred while retrieving the users."
                );
            }
        }

    }
}
