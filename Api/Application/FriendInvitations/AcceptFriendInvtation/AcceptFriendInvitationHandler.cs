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
        private readonly IFriendshipService _friendshipService;

        public AcceptFriendInvitationHandler(IFriendInvitationsService friendInvitationsService, IConversationService conversationService, IFriendshipService friendshipService)
        {
            _friendInvitationsService = friendInvitationsService;
            _conversationService = conversationService;
            _friendshipService = friendshipService;
        }

        public async Task<Result<AcceptFriendshipInviteDto>> Handle(AcceptFriendInvitationCommand request, CancellationToken cancellationToken)
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

            return new AcceptFriendshipInviteDto()
            {
                ConversationId = createConversationResult.Value.Id,
                FriendshipId = addFriendshipResult.Value
            };
        }
    }
}
