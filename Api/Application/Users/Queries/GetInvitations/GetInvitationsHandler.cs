using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Users.Queries.GetInvitations
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
                => await _friendInvitationsService.GetInvitationsSendedToUserAsync(request.UserId);
            
    }
}
