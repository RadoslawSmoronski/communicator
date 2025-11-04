using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Users;
using Domain.Entities;
using MediatR;
using Shared.Result;
using System;

namespace Application.Users.Queries.GetChats
{
    public class GetChatsHandler : IRequestHandler<GetChatsQuery, Result<List<GetChatsReadModel>>>
    {
        private readonly IConversationService _conversationService;
        private readonly IFriendshipService _friendshipService;
        private readonly IUserService _userService;

        public GetChatsHandler(IConversationService conversationService, IFriendshipService friendshipService, IUserService userService)
        {
            _conversationService = conversationService;
            _friendshipService = friendshipService;
            _userService = userService;
        }

        public async Task<Result<List<GetChatsReadModel>>> Handle(GetChatsQuery request, CancellationToken cancellationToken)
        {
            var conversationsResult = _conversationService.GetAll(request.UserId);
            if (!conversationsResult.IsSuccess)
                return conversationsResult.Error!;

            var conversations = conversationsResult.Value;

            if (conversations.Count == 0)
                return new List<GetChatsReadModel>();

            var friendsResult = await _friendshipService.GetUserFriendAsync(request.UserId);
            if (!friendsResult.IsSuccess)
                return friendsResult.Error!;

            var friends = friendsResult.Value;

            if (friends.Count == 0)
                return new List<GetChatsReadModel>();

            var friendIds = friends.Select(f => f.Id).ToList();

            var conversationsWithFriends = conversations.Where(c => friendIds.Contains(c.User1Id) || friendIds.Contains(c.User2Id));

            var result = conversationsWithFriends.Select(c =>
            {
                var friend = c.User1Id == request.UserId ? c.User2 : c.User1;

                return new GetChatsReadModel(
                    FriendId: friend!.Id,
                    FriendUserName: friend.UserName,
                    FriendAvatarUrl: friend.AvatarUrl,
                    ConversationId: c.Id,
                    LastMessageId: c.LastMessageId,
                    IsFriendSenderMessage: c.LastMessage?.SenderId == friend.Id,
                    LastMessageTimestamp: c.LastMessage?.Timestamp
                );
            });

            return result.ToList();
        }
    }
}