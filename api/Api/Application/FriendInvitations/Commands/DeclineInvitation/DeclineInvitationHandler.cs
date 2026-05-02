using Application.Common.Interfaces;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Commands.DeclineInvitation
{
    public class DeclineInvitationHandler(IFriendInvitationsService friendInvitationsService) : IRequestHandler<DeclineInvitationCommand, Result>
    {
        private readonly IFriendInvitationsService _friendInvitationsService = friendInvitationsService;

        public async Task<Result> Handle(DeclineInvitationCommand request, CancellationToken cancellationToken)
            => await _friendInvitationsService.DeleteAsync(request.InvitationId);
    }
}
