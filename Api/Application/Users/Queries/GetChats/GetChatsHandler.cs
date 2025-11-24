using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Users.Queries.GetChats
{
    public class GetChatsHandler(
        IConversationService conversationService,
        IFriendshipService friendshipService,
        IUsersConnectionService usersConnectionService,
        IUserAvatarService avatarService)
        : IRequestHandler<GetChatsQuery, Result<List<GetChatsReadModel>>>
    {
        private readonly IConversationService _conversationService = conversationService;
        private readonly IFriendshipService _friendshipService = friendshipService;
        private readonly IUsersConnectionService _usersConnectionService = usersConnectionService;
        private readonly IUserAvatarService _avatarService = avatarService;

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

            var onlineUsers = await _usersConnectionService.GetOnlineUsersIdAsync();

            var result = conversationsWithFriends.Select(c =>
            {
                var friend = c.User1Id == request.UserId ? c.User2 : c.User1;
                var friendIsOnline = onlineUsers.Any(id => id == friend!.Id);

                return new GetChatsReadModel(
                    FriendId: friend!.Id,
                    FriendUserName: friend.UserName,
                    FriendAvatarUrl: friend.AvatarUrl is not null ? _avatarService.GetPublicAvatarUrl(friend.AvatarUrl) : null,
                    ConversationId: c.Id,
                    IsFriendOnline: friendIsOnline,
                    LastMessageId: c.LastMessageId,
                    IsFriendSenderMessage: c.LastMessage?.SenderId == friend.Id,
                    LastMessageTimestamp: c.LastMessage?.CreatedAt
                );
            });

            return result.ToList();
        }
    }
}