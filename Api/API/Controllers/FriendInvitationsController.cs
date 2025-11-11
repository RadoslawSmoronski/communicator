using API.Contracts.FriendInvitations;
using Application.FriendInvitations.Commands.AcceptFriendInvtation;
using Application.FriendInvitations.Commands.DecelineInvitation;
using Application.Users.Commands.SendFriendInvitation;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/friend-invitations")]
    [ApiController]
    public class FriendInvitationsController(
        ISender sender,
        ILogger<FriendInvitationsController> logger,
        IMapper mapper)
        : BaseController(mapper, sender)
    {
        private readonly ILogger<FriendInvitationsController> _logger = logger;

        /// <summary>
        /// Send friendship invitation
        /// </summary>
        /// <param name="req">
        /// The request payload containing the sender and recipient identifiers.
        /// </param>
        /// <returns>
        /// Returns 200 OK with a <see cref="SendFriendInvitationResponse"/> containing the created invitation id;
        /// otherwise an error response produced by <c>HandleError</c>.
        /// </returns>
        /// <remarks>
        /// Route: POST api/friend-invitations
        /// Authorization: Required.
        /// Duplicate or conflicting invitations are handled by domain logic.
        /// </remarks>
        /// <response code="200">Invitation created successfully.</response>
        /// <response code="400">Invalid request payload or business rule violation.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">Sender not authorized to perform this action.</response>
        /// <response code="404">Sender or recipient user not found.</response>
        /// <response code="409">An active invitation or friendship already exists.</response>
        /// <response code="500">Unexpected server error.</response>
        [Authorize]
        [HttpPost()]
        [ProducesResponseType(typeof(SendFriendInvitationResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
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

        /// <summary>
        /// Delete friendship invitation
        /// </summary>
        /// <param name="friendInvitationId">The identifier of the friend invitation to decline.</param>
        /// <returns>
        /// Returns 200 OK when the invitation is successfully declined; otherwise an error response.
        /// </returns>
        /// <remarks>
        /// Route: PATCH api/friend-invitations/{friendInvitationId}/decline
        /// Authorization: Required.
        /// The operation changes the status of the invitation to &quot;Delete&quot; and may be idempotent depending on domain rules.
        /// </remarks>
        /// <response code="200">The invitation has been deleted.</response>
        /// <response code="400">Invalid request or the invitation cannot be deleted due to its current state.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">The user is not authorized to delete this invitation.</response>
        /// <response code="404">Invitation not found.</response>
        /// <response code="409">A conflict occurred (e.g., invitation already accepted or otherwise finalized).</response>
        /// <response code="500">Unexpected server error.</response>
        [Authorize]
        [HttpDelete("{friendInvitationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> DeclineInvitationAsync([FromRoute] Guid friendInvitationId)
        {
            var command = new DecelineInvitationCommand(friendInvitationId);
            var result = await _sender.Send(command);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleError(result, "FriendInvitationsController - DeclineInvitationAsync", _logger);
        }

        /// <summary>
        /// Accept friendship invitation
        /// </summary>
        /// <param name="friendInvitationId">The identifier of the friend invitation to accept.</param>
        /// <returns>
        /// Returns 200 OK with a <see cref="AcceptInvitationResponse"/> containing details of the created friendship;
        /// otherwise an error response.
        /// </returns>
        /// <remarks>
        /// Route: POST api/friend-invitations/{friendInvitationId}/accept
        /// Authorization: Required.
        /// Side effects: Creates a new friendship and consumes (removes or finalizes) the invitation.
        /// </remarks>
        /// <response code="200">The invitation was accepted and a friendship was created.</response>
        /// <response code="400">Invalid request or the invitation cannot be accepted in its current state.</response>
        /// <response code="401">Authentication is required.</response>
        /// <response code="403">The user is not authorized to accept this invitation.</response>
        /// <response code="404">Invitation not found.</response>
        /// <response code="409">A conflict occurred (e.g., invitation already handled or friendship already exists).</response>
        /// <response code="410">The invitation has expired.</response>
        /// <response code="500">Unexpected server error.</response>
        [Authorize]
        [HttpPatch("{friendInvitationId}/accept")]
        [ProducesResponseType(typeof(AcceptInvitationResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
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
