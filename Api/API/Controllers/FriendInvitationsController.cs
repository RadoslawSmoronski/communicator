using API.Contracts.FriendInvitations;
using API.DTOs;
using Application.Users.Commands.AcceptFriendInvtation;
using Application.Users.Commands.DecelineInvitation;
using Application.Users.Commands.SendFriendInvitation;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/friend-invitations")]
    [ApiController]
    public class FriendInvitationsController : BaseController
    {
        private readonly ISender _sender;
        private readonly ILogger<FriendInvitationsController> _logger;
        private readonly IMapper _mapper;

        public FriendInvitationsController(ISender sender, ILogger<FriendInvitationsController> logger, IMapper mapper)
        {
            _sender = sender;
            _logger = logger;
            _mapper = mapper;
        }

        [Authorize]  // refactor: docs
        [HttpPost()]
        public async Task<IActionResult> SendFriendInvitationAsync([FromBody] SendFriendInvitationRequest req)
        {
            var command = new SendFriendInvitationCommand(req.SenderId, req.RecipientId);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(new SendFriendInvitationResponse(FriendshipInvitationId: result.Value));
            }

            return HandleError(result, "FriendInvitationsController - SendFriendInvitationAsync", _logger);
        }

        [Authorize]  // refactor: docs
        [HttpPost("{friendInvitationId}/decline")]
        [HttpPatch("{friendInvitationId}/decline")]
        public async Task<IActionResult> DecelineInvitationAsync([FromRoute] Guid friendInvitationId)
        {
            var command = new DecelineInvitationCommand(friendInvitationId);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "FriendInvitationsController - DecelineInvitationAsync", _logger);
        }

        [Authorize]  // refactor: docs
        [HttpPost("{friendInvitationId}/accept")]
        [HttpPatch("{friendInvitationId}/accept")]
        public async Task<IActionResult> AcceptInvitationAsync([FromRoute] Guid friendInvitationId)
        {
            var command = new AcceptFriendInvitationCommand(friendInvitationId);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(_mapper.Map<AcceptInvitationResponse>(result.Value));
            }

            return HandleError(result, "FriendInvitationsController - AcceptInvitationAsync", _logger);
        }
    }
}
