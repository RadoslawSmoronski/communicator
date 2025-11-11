using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.Commands.SendFriendInvitation
{
    public class SendFriendInvitationHandler(
        IFriendInvitationsService friendInvitationsService,
        IFriendshipService friendshipService)
        : IRequestHandler<SendFriendInvitationCommand, Result<Guid>>
    {
        private readonly IFriendInvitationsService _friendInvitationsService = friendInvitationsService;
        private readonly IFriendshipService _friendshipService = friendshipService;

        public async Task<Result<Guid>> Handle(SendFriendInvitationCommand request, CancellationToken cancellationToken)
        {
            var isFriendshipExist = await _friendshipService.IsExistAsync(request.SenderId, request.RecipientId);

            if (isFriendshipExist.IsSuccess)
                return Error.Conflict("Friendship already exists", "Cannot send invitation.");

            if (isFriendshipExist.Error is not null && isFriendshipExist.Error.ErrorType != ErrorType.NotFound)
                return isFriendshipExist.Error;

            var result = await _friendInvitationsService.SendAsync(request.SenderId, request.RecipientId);

            if (result.IsSuccess)
                return result.Value;

            return result.Error!;
        }
    }
}
