using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using Application.FriendInvitations.Commands.AcceptFriendInvtation;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.AcceptFriendInvtation
{
    public class AcceptFriendInvitationHandler : IRequestHandler<AcceptFriendInvitationCommand, Result<AcceptFriendInvitationReadModel>>
    {
        private readonly IFriendInvitationsService _friendInvitationsService;
        private readonly IConversationService _conversationService;
        private readonly IFriendshipService _friendshipService;
        private readonly IUserService _userService;

        public AcceptFriendInvitationHandler(IFriendInvitationsService friendInvitationsService, IConversationService conversationService, IFriendshipService friendshipService, IUserService userService)
        {
            _friendInvitationsService = friendInvitationsService;
            _conversationService = conversationService;
            _friendshipService = friendshipService;
            _userService = userService;
        }

        public async Task<Result<AcceptFriendInvitationReadModel>> Handle(AcceptFriendInvitationCommand request, CancellationToken cancellationToken)
        {
            var friendshipInviteAcceptResult = await _friendInvitationsService.AcceptAsync(request.InvitationId);
            if (!friendshipInviteAcceptResult.IsSuccess)
                return friendshipInviteAcceptResult.Error!;

            var senderId = friendshipInviteAcceptResult.Value.SenderId;
            var recepientId = friendshipInviteAcceptResult.Value.RecipientId;

            var createConversationResult = await _conversationService.GetOrCreateAsync(senderId, recepientId);
            if (!createConversationResult.IsSuccess)
                return createConversationResult.Error!;

            var addFriendshipResult = await _friendshipService.AddAsync(senderId, recepientId);
            if (!addFriendshipResult.IsSuccess)
                return addFriendshipResult.Error!;

            return new AcceptFriendInvitationReadModel(
                    ConversationId: createConversationResult.Value.Id,
                    FriendshipId: addFriendshipResult.Value
                );
        }
    }
}
