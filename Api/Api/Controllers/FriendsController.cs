using Api.Data.IRepository;
using Api.Models;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Dtos.Controllers.UserController.RegisterAsync;
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
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _pendingFriendshipRepository.SendInviteAsync(sendInviteDto.SenderId, sendInviteDto.RecipientId);

                return Ok(new SendInviteOkResponseDto
                {
                    Succeeded = true,
                    Message = "Invitation successfully sent."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SendInviteFailedResponseDto
                {
                    Succeeded = false,
                    Errors = new List<string> { "An internal server error occurred." }
                });
            }
        }
    }
}
