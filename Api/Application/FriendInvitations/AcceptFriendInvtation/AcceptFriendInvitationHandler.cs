using Application.DTOs;
using Application.Interfaces;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.AcceptFriendInvtation
{
    public class AcceptFriendInvitationHandler : IRequestHandler<AcceptFriendInvitationCommand, Result<AcceptFriendshipInviteDto>>
    {
        private readonly IFriendInvitationsService _friendInvitationsService;

        public AcceptFriendInvitationHandler(IFriendInvitationsService friendInvitationsService)
        {
            _friendInvitationsService = friendInvitationsService;
        }

        public async Task<Result<AcceptFriendshipInviteDto>> Handle(AcceptFriendInvitationCommand request, CancellationToken cancellationToken)
        {
            var result = await _friendInvitationsService.AcceptInviteAsync(request.InvitationId);

            return new AcceptFriendshipInviteDto(); // refactor, add conversationd and friendship create functionalities
        }
    }
}
