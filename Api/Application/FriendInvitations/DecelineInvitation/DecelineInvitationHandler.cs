using Application.Common.Interfaces;
using Application.Interfaces;
using Application.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.DecelineInvitation
{
    public class DecelineInvitationHandler : IRequestHandler<DecelineInvitationCommand, Result>
    {
        private readonly IFriendInvitationsService _friendInvitationsService;

        public DecelineInvitationHandler(IUserService userService, IFriendInvitationsService friendInvitationsService)
        {
            _friendInvitationsService = friendInvitationsService;
        }

        public Task<Result> Handle(DecelineInvitationCommand request, CancellationToken cancellationToken)
            => _friendInvitationsService.DeleteInviteAsync(request.InvitationId);
    }
}
