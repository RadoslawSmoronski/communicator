using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Users.Queries.GetInvitations
{
    public class GetInvitationsHandler : IRequestHandler<GetInvitationsCommand, Result<List<GetInvitationsReadModel>>>
    {
        private readonly IFriendInvitationsService _friendInvitationsService;
        private readonly IUserService _userService;

        public GetInvitationsHandler(IUserService userService, IFriendInvitationsService friendInvitationsService)
        {
            _userService = userService;
            _friendInvitationsService = friendInvitationsService;
        }

        public async Task<Result<List<GetInvitationsReadModel>>> Handle(GetInvitationsCommand request, CancellationToken cancellationToken)
                => await _friendInvitationsService.GetInvitationsSendedToUserAsync(request.UserId);
            
    }
}
