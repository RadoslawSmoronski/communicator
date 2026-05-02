using Application.Friendships.Commands.DeleteFriendship;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

namespace API.Controllers
{
    [Route("api/friendships")]
    [ApiController]
    public class FriendshipsController(IMapper mapper, ISender sender, ILogger<FriendshipsController> logger) : BaseController(mapper, sender)
    {
        private readonly ILogger<FriendshipsController> _logger = logger;

        /// <summary>
        /// Delete friendship
        /// </summary>
        /// <param name="friendshipId">The identifier of the friendship to delete.</param>
        /// <returns>
        /// Returns 200 OK when the friendship is successfully deleted; otherwise an error response.
        /// </returns>
        /// <remarks>
        /// Route: DELETE api/friendships/{friendshipId}
        /// Authorization: Required.
        /// The operation removes the friendship relationship between the users.
        /// </remarks>
        /// <response code="200">The friendship was deleted.</response>
        /// <response code="400">Invalid identifier or request cannot be processed.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">The user is not authorized to delete this friendship.</response>
        /// <response code="404">Friendship not found.</response>
        /// <response code="409">Conflict occurred (e.g., pending operations prevent deletion).</response>
        /// <response code="500">Unexpected server error.</response>
        [Authorize]
        [HttpDelete("{friendshipId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> DeleteFriendshipAsync([FromRoute] Guid friendshipId)
        {
            var command = new DeleteFriendshipCommand(friendshipId);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "FriendsController - DeleteFriendshipAsync", _logger);
        }
    }
}
