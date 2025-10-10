using Application.Auth.Commands.ResetPassword;
using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Users;
using Application.Users.Commands.DeleteAvatar;
using Domain.Entities;
using MediatR;
using Shared.Result;
using System.Net;

namespace Application.FriendInvitations.SendFriendInvitation
{
    public class SendFriendInvitationHandler : IRequestHandler<SendFriendInvitationCommand, Result<SendFriendInvitationDto>>
    {
        private readonly IUserService _userService;
        private readonly IFriendInvitationsService _friendInvitationsService;

        public SendFriendInvitationHandler(IUserService userService, IFriendInvitationsService friendInvitationsService)
        {
            _userService = userService;
            _friendInvitationsService = friendInvitationsService;
        }

        public async Task<Result<SendFriendInvitationDto>> Handle(SendFriendInvitationCommand request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.SenderId))
            {
                return Error.Unauthorized("Unauthorized", "User is not authorized.");
            }

            var result = await _friendInvitationsService.SendAsync(request.SenderId, request.RecipientId);

            if(result.IsSuccess)
            {
                return new SendFriendInvitationDto()
                {
                    FriendshipInvitationId = result.Value
                };
            }

            return result.Error!;
        }
    }
}
