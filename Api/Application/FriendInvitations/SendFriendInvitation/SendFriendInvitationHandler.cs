using Application.Auth.Commands.ResetPassword;
using Application.Interfaces;
using Application.Interfaces.Users;
using Application.Users.Commands.DeleteAvatar;
using MediatR;
using Shared.Result;
using System.Net;

namespace Application.FriendInvitations.SendFriendInvitation
{
    public class SendFriendInvitationHandler : IRequestHandler<SendFriendInvitationCommand, Result<Guid>>
    {
        private readonly IUserService _userService;
        private readonly IFriendInvitationsService _friendInvitationsService;

        public SendFriendInvitationHandler(IUserService userService, IFriendInvitationsService friendInvitationsService)
        {
            _userService = userService;
            _friendInvitationsService = friendInvitationsService;
        }

        public async Task<Result<Guid>> Handle(SendFriendInvitationCommand request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.SenderId))
            {
                return Error.Unauthorized("Unauthorized", "User is not authorized.");
            }

            return await _friendInvitationsService.SendInviteAsync(request.SenderId, request.RecipientId);
        }
    }
}
