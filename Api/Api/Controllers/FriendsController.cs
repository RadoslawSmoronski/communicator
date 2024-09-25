using Api.Data.IRepository;
using Api.Models;
using Api.Models.Dtos.Controllers.FriendsController;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class FriendsController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IPendingFriendshipRepository _pendingFriendshipRepository;

        public FriendsController(UserManager<UserAccount> userManager, IPendingFriendshipRepository pendingFriendshipRepository)
        {
            _userManager = userManager;
            _pendingFriendshipRepository = pendingFriendshipRepository;
        }


        [HttpPost("sendInviteAsync")]
        public async Task<IActionResult> SendInviteAsync(SendInviteDto sendInviteDto)
        {
            if(ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _pendingFriendshipRepository.SendInviteAsync(sendInviteDto.SenderId, sendInviteDto.RecipientId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
