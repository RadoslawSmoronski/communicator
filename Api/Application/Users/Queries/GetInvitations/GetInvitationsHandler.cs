using Application.Common.Interfaces;
using MediatR;
using Shared.Result;

namespace Application.Users.Queries.GetInvitations
{
    public class GetInvitationsHandler(IFriendInvitationsService friendInvitationsService) : IRequestHandler<GetInvitationsCommand, Result<List<GetInvitationsReadModel>>>
    {
        private readonly IFriendInvitationsService _friendInvitationsService = friendInvitationsService;

        public async Task<Result<List<GetInvitationsReadModel>>> Handle(GetInvitationsCommand request, CancellationToken cancellationToken)
                => await _friendInvitationsService.GetInvitationsSendedToUserAsync(request.UserId);  
    }
}
