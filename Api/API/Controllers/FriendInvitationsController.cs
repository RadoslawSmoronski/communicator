using API.DTOs;
using Application.DTOs;
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
        public async Task<IActionResult> SendFriendInvitationAsync([FromBody] InviteDto inviteDto)
        {
            var command = new SendFriendInvitationCommand(inviteDto.SenderId, inviteDto.RecipientId);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleError(result, "FriendInvitationsController - SendFriendInvitationAsync", _logger);
        }
    }
}
