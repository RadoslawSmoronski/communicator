using Application.DTOs;
using Application.Interfaces;
using MediatR;
using Shared.Result;

namespace Application.FriendInvitations.AcceptFriendInvtation
{
    public class AcceptFriendInvitationHandler : IRequestHandler<AcceptFriendInvitationCommand, Result<AcceptFriendshipInviteDto>>
    {
        private readonly IFriendInvitationsService _friendInvitationsService;
        private readonly IConversationService _conversationService;

        public AcceptFriendInvitationHandler(IFriendInvitationsService friendInvitationsService, IConversationService conversationService)
        {
            _friendInvitationsService = friendInvitationsService;
            _conversationService = conversationService;
        }

        public async Task<Result<AcceptFriendshipInviteDto>> Handle(AcceptFriendInvitationCommand request, CancellationToken cancellationToken)
        {
            var friendshipInviteAcceptResult = await _friendInvitationsService.AcceptInviteAsync(request.InvitationId);
            if (!friendshipInviteAcceptResult.IsSuccess)
                return friendshipInviteAcceptResult.Error!;

            var senderId = friendshipInviteAcceptResult.Value.SenderId;
            var recepientId = friendshipInviteAcceptResult.Value.RecipientId;

            var createConversationResult = await _conversationService.GetOrCreateAsync(senderId, recepientId);
            if (!createConversationResult.IsSuccess)
                return createConversationResult.Error!;

            return new AcceptFriendshipInviteDto() // refactor, add conversationd and friendship create functionalities
            { ConversationId = createConversationResult.Value.Id };
        }
    }
}
