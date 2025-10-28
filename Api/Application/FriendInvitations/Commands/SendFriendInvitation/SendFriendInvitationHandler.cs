using Application.Auth.Commands.ResetPassword;
using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Commands.SendFriendInvitation
{
    public class SendFriendInvitationHandler : IRequestHandler<SendFriendInvitationCommand, Result<SendFriendInvitationDto>>
    {
        private readonly IUserService _userService;
        private readonly IFriendInvitationsService _friendInvitationsService;
        private readonly IFriendshipService _friendshipService;

        public SendFriendInvitationHandler(IUserService userService, IFriendInvitationsService friendInvitationsService, IFriendshipService friendshipService)
        {
            _userService = userService;
            _friendInvitationsService = friendInvitationsService;
            _friendshipService = friendshipService;
        }

        public async Task<Result<SendFriendInvitationDto>> Handle(SendFriendInvitationCommand request, CancellationToken cancellationToken)
        {
            var isFriendshipExist = await _friendshipService.IsExistAsync(request.SenderId, request.RecipientId);

            if (isFriendshipExist.IsSuccess)
                return Error.Conflict("Friendship already exists", "Cannot send invitation.");

            if (isFriendshipExist.Error is not null && isFriendshipExist.Error.ErrorType != ErrorType.NotFound)
                return isFriendshipExist.Error;

            var result = await _friendInvitationsService.SendAsync(request.SenderId, request.RecipientId);

            if (result.IsSuccess)
                return new SendFriendInvitationDto
                {
                    FriendshipInvitationId = result.Value
                };

            return result.Error!;
        }
    }
}
