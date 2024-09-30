using Api.Data.IRepository;
using Api.Exceptions.PendingfriendshipRepository;
using Api.Exceptions;
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
            if (!ModelState.IsValid)
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
            catch (EnteredDataIsNullException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(CreateErrorResponse(ex.Message));
            }
            catch (FriendshipPendingIsAlreadyExistException ex)
            {
                return Conflict(CreateErrorResponse(ex.Message));
            }
            catch (DatabaseOperationException ex)
            {
                return StatusCode(500, CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, CreateErrorResponse("An internal server error occurred."));
            }
        }

        private SendInviteFailedResponseDto CreateErrorResponse(string message)
        {
            return new SendInviteFailedResponseDto
            {
                Succeeded = false,
                Message = message
            };
        }
    }
}
