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
using Api.Managers;
using Api.Utilities.Result;
using Microsoft.AspNetCore.Http.HttpResults;
using Api.Models.Dtos.Chat;

namespace Api.Controllers
{
    [Route("api/friends")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ResponseHttpFactory _responseHttpFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IChatManager _chatManager;

        public ChatController(
            IMapper mapper,
            ResponseHttpFactory responseHttpFactory,
            IHttpContextAccessor httpContextAccessor,
            IChatManager chatManager    )
        {
            _mapper = mapper;
            _responseHttpFactory = responseHttpFactory;
            _httpContextAccessor = httpContextAccessor;
            _chatManager = chatManager;
        }

        [Authorize]
        [HttpGet("getChats")]
        public async Task<IActionResult> GetChatsAsync()
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

            var result = await _chatManager.GetChatsAsync(userId);

            if (result.IsSuccess)
            {
                var responseOk = _responseHttpFactory.Create<List<ChatDto>>
                    (ResponseHttpType.Success,
                    "Chat/s was/were found.",
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
        [HttpGet("getMessages")]
        public async Task<IActionResult> GetMessagesAsync(string conversationId)
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

            var result = await _chatManager.GetMessagesAsync(conversationId);

            if (result.IsSuccess)
            {
                var responseOk = _responseHttpFactory.Create<List<MessageDto>>
                    (ResponseHttpType.Success,
                    "TEXT TO REFACTOR.",
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
