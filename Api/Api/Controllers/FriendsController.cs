using Api.Data.IRepository;
using Api.Exceptions.FriendshipInvitationRepository;
using Api.Exceptions;
using Api.Models;
using Api.Models.Dtos.Controllers.FriendsController;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Api.Data.Repository;
using Api.Managers.Interfaces;
using Api.Managers;
using AutoMapper;
using Api.Models.Dtos.Responses.Interfaces;
using Api.Models.Dtos.Responses;

namespace Api.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class FriendsController : Controller
    {
        private readonly IFriendsManager _friendsManager;
        private readonly IMapper _mapper;
        private readonly ResponseHttpFactory _responseHttpFactory;

        public FriendsController(IFriendsManager friendsManager,
            IMapper mapper,
            ResponseHttpFactory responseHttpFactory)
        {
            _friendsManager = friendsManager;
            _mapper = mapper;
            _responseHttpFactory = responseHttpFactory;
        }

        //[Authorize]
        [HttpPost("sendInviteAsync")]
        public async Task<IActionResult> SendInviteAsync(SendInviteDto sendInviteDto)
        {
            var inviteResult = await _friendsManager.SendInviteAsync(sendInviteDto.SenderId, sendInviteDto.RecipientId);

            if (inviteResult.IsSuccess)
            {
                var responseOk = _responseHttpFactory.Create(ResponseHttpType.Success, "Invitation sent.");
                return Ok(responseOk);
            }

            if (inviteResult.Error != null)
            {
                var errorType = _mapper.Map<ResponseHttpType>(inviteResult.Error.ErrorType);
                var response = _responseHttpFactory.Create(errorType, inviteResult.Error.Description);
                return StatusCode(response.Status, response);
            }

            var fallbackResponse = _responseHttpFactory.Create(ResponseHttpType.InternalServerError, "An unexpected error occurred.");
            return StatusCode(500, fallbackResponse);
        }

        //[Authorize]
        [HttpGet("getInvitaties/{userId}")]
        public async Task<IActionResult> GetInvitiesAsync(string userId)
        {
            return Ok();

            //if (String.IsNullOrEmpty(userId) || userId.Length != 36)
            //{
            //    return BadRequest(CreateErrorResponse("userId must be exactly 36 characters long."));
            //}

            //try
            //{
            //    var invities = await _friendshipInvitationRepository.GetInvitiesAsync(userId);

            //    return Ok(new GetInvitiesOkResponseDto
            //    {
            //        Succeeded = true,
            //        Message = "Successfully found invitations.",
            //        FriendshipInvitations = invities
            //    });
            //}
            //catch (EnteredDataIsNullException ex)
            //{
            //    return BadRequest(CreateErrorResponse(ex.Message));
            //}
            //catch (UserNotFoundException ex)
            //{
            //    return NotFound(CreateErrorResponse(ex.Message));
            //}
            //catch (FriendshipInvitationDoesNotExistException ex)
            //{
            //    return NotFound(CreateErrorResponse(ex.Message));
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500, CreateErrorResponse("An internal server error occurred."));
            //}
        }

        //[Authorize]
        //[HttpPost("decelineInvite")]
        //public async Task<IActionResult> DecelineInviteAsync(DecelineInviteDto decelineInviteDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    try
        //    {
        //        await _friendshipInvitationRepository.DeleteInviteAsync(decelineInviteDto.SenderId, decelineInviteDto.RecipientId);

        //        return Ok(new DecelineInviteOkResponse
        //        {
        //            Succeeded = true,
        //            Message = "Decelined friend request.",
        //        });
        //    }
        //    catch (EnteredDataIsNullException ex)
        //    {
        //        return BadRequest(CreateErrorResponse(ex.Message));
        //    }
        //    catch (UserNotFoundException ex)
        //    {
        //        return NotFound(CreateErrorResponse(ex.Message));
        //    }
        //    catch (FriendshipInvitationDoesNotExistException ex)
        //    {
        //        return NotFound(CreateErrorResponse(ex.Message));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, CreateErrorResponse("An internal server error occurred."));
        //    }
        //}

        //[Authorize]
        //[HttpPost("acceptInvite")]
        //public async Task<IActionResult> AcceptInviteAsync(AcceptInviteDto acceptInviteDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    try
        //    {

        //        await _friendshipInvitationRepository.DeleteInviteAsync(acceptInviteDto.SenderId, acceptInviteDto.RecipientId);
        //        await _friendshipRepository.AddFriendshipAsync(acceptInviteDto.SenderId, acceptInviteDto.RecipientId);

        //        return Ok(new AcceptInviteOkResponse
        //        {
        //            Succeeded = true,
        //            Message = "Accepted friend request.",
        //        });
        //    }
        //    catch (EnteredDataIsNullException ex)
        //    {
        //        return BadRequest(CreateErrorResponse(ex.Message));
        //    }
        //    catch (UserNotFoundException ex)
        //    {
        //        return NotFound(CreateErrorResponse(ex.Message));
        //    }
        //    catch (FriendshipInvitationDoesNotExistException ex)
        //    {
        //        return NotFound(CreateErrorResponse(ex.Message));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, CreateErrorResponse("An internal server error occurred."));
        //    }
        //}

        //[Authorize]
        //[HttpGet("getFriends/{userId}")]
        //public async Task<IActionResult> GetFriendsAsync(string userId)
        //{
        //    if (String.IsNullOrEmpty(userId) || userId.Length != 36)
        //    {
        //        return BadRequest(CreateErrorResponse("userId must be exactly 36 characters long."));
        //    }

        //    try
        //    {
        //        var invities = await _friendshipRepository.GetFriendsAsync(userId);

        //        return Ok(new GetFriendsOkResponseDto
        //        {
        //            Succeeded = true,
        //            Message = "Successfully found friends.",
        //            Friends = invities
        //        });
        //    }
        //    catch (EnteredDataIsNullException ex)
        //    {
        //        return BadRequest(CreateErrorResponse(ex.Message));
        //    }
        //    catch (UserNotFoundException ex)
        //    {
        //        return NotFound(CreateErrorResponse(ex.Message));
        //    }
        //    catch (FriendshipInvitationDoesNotExistException ex)
        //    {
        //        return NotFound(CreateErrorResponse(ex.Message));
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, CreateErrorResponse("An internal server error occurred."));
        //    }
        //}

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
