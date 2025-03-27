using Api.Models.Dtos.Controllers.FriendsController;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Api.Managers.Interfaces;
using AutoMapper;
using Api.Models.Dtos.Responses.Interfaces;
using Api.Models.Dtos.Responses;
using Api.Models.Dtos;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class FriendsController : Controller
    {
        private readonly IFriendsManager _friendsManager;
        private readonly IMapper _mapper;
        private readonly ResponseHttpFactory _responseHttpFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FriendsController(IFriendsManager friendsManager,
            IMapper mapper,
            ResponseHttpFactory responseHttpFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _friendsManager = friendsManager;
            _mapper = mapper;
            _responseHttpFactory = responseHttpFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize]
        [HttpPost("sendInviteAsync")]
        public async Task<IActionResult> SendInviteAsync(InviteDto inviteDto)
        {
            var inviteResult = await _friendsManager.SendInviteAsync(inviteDto.SenderId, inviteDto.RecipientId);

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

        [Authorize]
        [HttpGet("getInvitations/{userId}")]
        public async Task<IActionResult> GetInvitationsAsync(string userId)
        {
            if (!Guid.TryParse(userId, out Guid resultGuid))
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.BadRequest, "UserId not valid format.");

                return BadRequest(response);
            }

            var result = await _friendsManager.GetInvitationsAsync(userId);

            if (result.IsSuccess)
            {
                var responseOk = _responseHttpFactory.Create<List<SimpleUserDto>>
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

        [Authorize]
        [HttpGet("getUsersToInviteByText/{text}")]
        public async Task<IActionResult> GetUsersToInviteByTextAsync(string text)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return StatusCode(500, _responseHttpFactory.Create(
                    ResponseHttpType.InternalServerError,
                    "JWT Token error."));
            }


            if (!Guid.TryParse(userId, out Guid resultGuid))
            {
                var response = _responseHttpFactory.Create
                               (ResponseHttpType.BadRequest, "UserId not valid format.");

                return BadRequest(response);
            }

            var result = await _friendsManager.GetUsersToInviteByTextAsync(userId, text);

            if (result.IsSuccess)
            {
                var responseOk = _responseHttpFactory.Create<List<UserToInviteDto>>
                    (ResponseHttpType.Success,
                    "User to invite found.",
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

        [Authorize]
        [HttpPost("decelineInvite")]
        public async Task<IActionResult> DecelineInviteAsync(InviteDto inviteDto)
        {
            var result = await _friendsManager.DecelineInviteAsync(inviteDto.SenderId, inviteDto.RecipientId);

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

        [Authorize]
        [HttpPost("acceptInvite")]
        public async Task<IActionResult> AcceptInviteAsync(InviteDto acceptInviteDto)
        {
            var result = await _friendsManager.AddFriendsAsync(acceptInviteDto.SenderId, acceptInviteDto.RecipientId);

            if (result.IsSuccess)
            {
                var responseOk = _responseHttpFactory.Create(ResponseHttpType.Success, "Friend has been successfully added.");
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

        [Authorize]
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
                var responseOk = _responseHttpFactory.Create<List<SimpleUserDto>>
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
    }
}
