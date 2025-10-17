using Application.DTOs;
using Application.FriendInvitations.GetInvitations;
using Application.Interfaces;
using Application.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.GetInvitationsInvitation
{
    public class GetInvitationsHandler : IRequestHandler<GetInvitationsCommand, Result<List<FriendshipInvitationDto>>>
    {
        private readonly IFriendInvitationsService _friendInvitationsService;
        private readonly IUserService _userService;

        public GetInvitationsHandler(IUserService userService, IFriendInvitationsService friendInvitationsService)
        {
            _userService = userService;
            _friendInvitationsService = friendInvitationsService;
        }

        public async Task<Result<List<FriendshipInvitationDto>>> Handle(GetInvitationsCommand request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.UserId))
            {
                return Error.Unauthorized("Unauthorized", "User is not authorized.");

            }
            var result = await _friendInvitationsService.GetInvitationsSendedToUserAsync(request.UserId);

            if (result.IsSuccess)
                return result.Value;

            return result.Error!;
        }
            
    }
}
