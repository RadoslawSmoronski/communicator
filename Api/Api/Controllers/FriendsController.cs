using Api.Data.IRepository;
using Api.Exceptions.FriendshipInvitationRepository;
using Api.Exceptions;
using Api.Models;
using Api.Models.Dtos.Controllers.FriendsController;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Api.Data.Repository;

namespace Api.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class FriendsController : Controller
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly IFriendshipInvitationsRepository _friendshipInvitationRepository;

        public FriendsController(UserManager<UserAccount> userManager, IFriendshipInvitationsRepository friendshipInvitationRepository)
        {
            _userManager = userManager;
            _friendshipInvitationRepository = friendshipInvitationRepository;
        }


        [HttpPost("sendInviteAsync")]
        //[Authorize]
        public async Task<IActionResult> SendInviteAsync(SendInviteDto sendInviteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _friendshipInvitationRepository.SendInviteAsync(sendInviteDto.SenderId, sendInviteDto.RecipientId);

                return Ok(new SendInviteOkResponseDto
                {
                    Succeeded = true,
                    Message = "Invitation successfully sent."
                });
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(CreateErrorResponse(ex.Message));
            }
            catch (FriendshipInvitationIsAlreadyExistException ex)
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

        [HttpGet("getUserInvitaties/{userId}")]
        public async Task<IActionResult> GetUserInvitiesAsync(string userId)
        {
            if (String.IsNullOrEmpty(userId) || userId.Length != 36)
            {
                return BadRequest(CreateErrorResponse("userId must be exactly 36 characters long."));
            }

            try
            {
                var invities = await _friendshipInvitationRepository.GetUserInvitiesAsync(userId);

                return Ok(new GetInvitiesOkResponseDto
                {
                    Succeeded = true,
                    Message = "Successfully found invitations.",
                    FriendshipInvitations = invities
                });
            }
            catch (EnteredDataIsNullException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(CreateErrorResponse(ex.Message));
            }
            catch (FriendshipInvitationDoesNotExistException ex)
            {
                return NotFound(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, CreateErrorResponse("An internal server error occurred."));
            }
        }

        [HttpPost("decelineInvite")]
        public async Task<IActionResult> DecelineInviteAsync(DecelineInviteDto decelineInviteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _friendshipInvitationRepository.DecelineInviteAsync(decelineInviteDto.RecipientId, decelineInviteDto.SenderId);

                return Ok(new DecelineInviteOkResponse
                {
                    Succeeded = true,
                    Message = "Decelined friend request.",
                });
            }
            catch (EnteredDataIsNullException ex)
            {
                return BadRequest(CreateErrorResponse(ex.Message));
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(CreateErrorResponse(ex.Message));
            }
            catch (FriendshipInvitationDoesNotExistException ex)
            {
                return NotFound(CreateErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, CreateErrorResponse("An internal server error occurred."));
            }
        }

        private FriendsFailedResponseDto CreateErrorResponse(string message)
        {
            return new FriendsFailedResponseDto
            {
                Succeeded = false,
                Message = message
            };
        }

    }
}
