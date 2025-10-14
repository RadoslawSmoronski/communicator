using API.DTOs;
using Application.Friendships.Commands.DeleteFriendship;
using Application.Users.Commands.RegisterUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/friendships")]
    [ApiController]
    public class FriendshipsController : BaseController
    {
        private readonly ISender _sender;
        private readonly ILogger<FriendshipsController> _logger;

        public FriendshipsController(ISender sender, ILogger<FriendshipsController> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        [Authorize]
        [HttpDelete("{friendshipId}")]
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
