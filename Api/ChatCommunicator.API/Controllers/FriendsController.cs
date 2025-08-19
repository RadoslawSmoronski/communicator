using ChatCommunicator.API.Controllers;
using ChatCommunicator.Application.Hubs;
using ChatCommunicator.Application.Hubs.Interfaces;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Contracts.Dtos.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatCommunicator.Application.Controllers
{
    [Route("api/friendships")]
    [ApiController]
    public class FriendsController : BaseController
    {
        private readonly IFriendsService _friendsService;
        private readonly ILogger<FriendsController> _logger;   

        public FriendsController(IFriendsService friendsService, ILogger<FriendsController> logger)
        {
            _friendsService = friendsService;
            _logger = logger;
        }

        /// <summary>
        /// Delete friendship
        /// </summary>
        /// <remarks>
        /// This endpoint deletes a friendship relationship identified by <c>friendshipId</c>.
        /// Requires a valid JWT token in the Authorization header.
        /// If the friendship does not exist or the user does not have permission, an error response is returned.
        /// </remarks>
        /// <param name="friendshipId">The ID of the friendship (GUID). Required.</param>
        /// <returns>
        /// No content on success or an error response.
        /// </returns>
        /// <response code="204">Successfully deleted the friendship.</response>
        /// <response code="400">Invalid input or validation error.</response>
        /// <response code="401">Unauthorized – missing or invalid JWT token.</response>
        /// <response code="404">Friendship not found or access denied.</response>
        /// <response code="500">Internal server error.</response>
        /// <example>
        /// DELETE /api/friendships/{friendshipId}
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
        /// </example> 
        [Authorize]
        [HttpDelete("{friendshipId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteFriendshipAsync([FromRoute]Guid friendshipId)
        {
            var validate = ValidateAndGetUserId("DeleteFriendshipAsync", _logger, out Guid userId);

            if (validate != null)
            {
                return validate;
            }

            _logger.LogInformation("Deleting friendship with ID {FriendshipId}", friendshipId);

            var result = await _friendsService.DeleteAsync(friendshipId);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            return HandleError(result, "DeleteFriendshipAsync", _logger);
        }

    }
}
