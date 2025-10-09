using API.DTOs;
using Application.DTOs;
using Application.FriendInvitations.AcceptFriendInvtation;
using Application.FriendInvitations.DecelineInvitation;
using Application.FriendInvitations.SendFriendInvitation;
using Application.Users.Commands.RegisterUser;
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

        public FriendInvitationsController(ISender sender, ILogger<FriendInvitationsController> logger)
        {
            _sender = sender;
            _logger = logger;
        }

        [Authorize]  // refactor: docs
        [HttpPost()]
        public async Task<IActionResult> SendFriendInvitationAsync([FromBody] FriendInvitationDto friendInvitationDto)
        {
            var command = new SendFriendInvitationCommand(friendInvitationDto.SenderId, friendInvitationDto.RecipientId);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
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
                return Ok(result.Value);
            }

            return HandleError(result, "FriendInvitationsController - AcceptInvitationAsync", _logger);
        }
    }
}
