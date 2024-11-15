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
        [HttpGet("getInvitations/{userId}")]
        public async Task<IActionResult> GetInvitationsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.BadRequest, "UserId is required.");

                return BadRequest(response);
            }

            if (!Guid.TryParse(userId, out Guid resultGuid))
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.BadRequest, "UserId not valid format.");

                return BadRequest(response);
            }

            var result = await _friendsManager.GetInvitationsAsync(userId);

            if (result.IsSuccess)
            {
                var responseOk = _responseHttpFactory.Create<List<GetInvitationsUserDto>>
                    (ResponseHttpType.Success,
                    "Invitations found.",
                    result.Value);

                return Ok(responseOk);
            }

            if (result.Error != null)
            {
                var errorType = _mapper.Map<ResponseHttpType>(result.Error.ErrorType);
                var response = _responseHttpFactory.Create(errorType, result.Error.Description);
                return StatusCode(response.Status, response);
            }

            var fallbackResponse = _responseHttpFactory.Create(ResponseHttpType.InternalServerError, "An unexpected error occurred.");
            return StatusCode(500, fallbackResponse);
        }

        //[Authorize]
        [HttpPost("decelineInvite")]
        public async Task<IActionResult> DecelineInviteAsync(DecelineInviteDto decelineInviteDto)
        {
            var result = await _friendsManager.DecelineInviteAsync(decelineInviteDto.SenderId, decelineInviteDto.RecipientId);

            if (result.IsSuccess)
            {
                var responseOk = _responseHttpFactory.Create(ResponseHttpType.Success, "Invitation decelined.");
                return Ok(responseOk);
            }

            if (result.Error != null)
            {
                var errorType = _mapper.Map<ResponseHttpType>(result.Error.ErrorType);
                var response = _responseHttpFactory.Create(errorType, result.Error.Description);
                return StatusCode(response.Status, response);
            }

            var fallbackResponse = _responseHttpFactory.Create(ResponseHttpType.InternalServerError, "An unexpected error occurred.");
            return StatusCode(500, fallbackResponse);
        }

        //[Authorize]
        [HttpPost("acceptInvite")]
        public async Task<IActionResult> AcceptInviteAsync(AcceptInviteDto acceptInviteDto)
        {
            var result = await _friendsManager.AddFriendsAsync(acceptInviteDto.SenderId, acceptInviteDto.RecipientId);

            if (result.IsSuccess)
            {
                var responseOk = _responseHttpFactory.Create(ResponseHttpType.Success, "Friend successfully added.");
                return Ok(responseOk);
            }

            if (result.Error != null)
            {
                var errorType = _mapper.Map<ResponseHttpType>(result.Error.ErrorType);
                var response = _responseHttpFactory.Create(errorType, result.Error.Description);
                return StatusCode(response.Status, response);
            }

            var fallbackResponse = _responseHttpFactory.Create(ResponseHttpType.InternalServerError, "An unexpected error occurred.");
            return StatusCode(500, fallbackResponse);
        }

        //[Authorize]
        [HttpGet("getFriends/{userId}")]
        public async Task<IActionResult> GetFriendsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.BadRequest, "UserId is required.");

                return BadRequest(response);
            }

            if (!Guid.TryParse(userId, out Guid resultGuid))
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.BadRequest, "UserId not valid format.");

                return BadRequest(response);
            }

            var result = await _friendsManager.GetFriendsAsync(userId);

            if (result.IsSuccess)
            {
                var responseOk = _responseHttpFactory.Create<List<FriendDto>>
                    (ResponseHttpType.Success,
                    "Friends found.",
                    result.Value);

                return Ok(responseOk);
            }

            if (result.Error != null)
            {
                var errorType = _mapper.Map<ResponseHttpType>(result.Error.ErrorType);
                var response = _responseHttpFactory.Create(errorType, result.Error.Description);
                return StatusCode(response.Status, response);
            }

            var fallbackResponse = _responseHttpFactory.Create(ResponseHttpType.InternalServerError, "An unexpected error occurred.");
            return StatusCode(500, fallbackResponse);
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
