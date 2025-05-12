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
    [Route("api/chat")]
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

        /// <summary>
        /// GetChats.
        /// </summary>
        /// <remarks>
        /// This endpoint returns all chat conversations that belong to the currently authenticated user.
        /// A valid JWT token must be provided in the Authorization header. The user's ID is extracted from
        /// the token claims and used to fetch associated chat data.
        /// 
        /// If the user's ID is missing or not a valid GUID, a 401 Unauthorized response is returned.
        /// If a validation or internal error occurs, appropriate error responses are provided.
        /// </remarks>
        /// <returns>
        /// A list of chat conversations associated with the authenticated user, or an error response.
        /// </returns>
        /// <response code="200">Returns the list of chats.</response>
        /// <response code="400">Invalid input or validation failure.</response>
        /// <response code="401">Unauthorized – missing or invalid JWT token.</response>
        /// <response code="500">Unexpected server error occurred.</response>
        /// <example>
        /// GET /api/user/getChats
        /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
        /// </example>
        [Authorize]
        [HttpGet("getChats")]
        [ProducesResponseType(typeof(List<ChatDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetChatsAsync()
        {
            var userClaimId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userClaimId == null)
            {
                return Problem(
                    statusCode: 401,
                    title: "Unauthorized",
                    detail: "JWT Token is not valid."
                );
            }


            if (!Guid.TryParse(userClaimId, out Guid userId))
            {
                return Problem(
                    statusCode: 401,
                    title: "Unauthorized",
                    detail: "UserId from JWT Token is not a guid."
                );
            }

            var result = await _chatManager.GetChatsAsync(userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Validation)
                {
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: errorMessage
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: "An unexpected error occurred."
                );
            }

            return Problem(
                statusCode: 500,
                title: "InternalServerError",
                detail: "An unexpected error occurred."
            );
        }


        [Authorize]
        [HttpGet("getPagedMessages")]
        [ProducesResponseType(typeof(List<MessageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagedMessagesAsync([FromQuery] GetPagedMessagesDto getMessagesDto)
        {
            var result = await _chatManager.GetPagedMessagesFromMessageIdAsync(getMessagesDto.ConversationId, getMessagesDto.FromMessageId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            if (result.Error != null)
            {
                var errorCode = result.Error.ErrorType;
                var errorMessage = result.Error.Description;

                if (errorCode == ErrorType.Validation)
                {
                    return Problem(
                        statusCode: 400,
                        title: "Bad Request",
                        detail: errorMessage
                    );
                }
                else if (errorCode == ErrorType.NotFound)
                {
                    return Problem(
                        statusCode: 404,
                        title: "Not Found",
                        detail: errorMessage
                    );
                }

                return Problem(
                    statusCode: 500,
                    title: "InternalServerError",
                    detail: "An unexpected error occurred."
                );
            }

            return Problem(
                statusCode: 500,
                title: "InternalServerError",
                detail: "An unexpected error occurred."
            );
        }

    }
}
