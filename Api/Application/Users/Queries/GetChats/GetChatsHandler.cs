using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Users;
using MediatR;
using Shared.Result;
using System;

namespace Application.Users.Queries.GetChats
{
    public class GetChatsHandler : IRequestHandler<GetChatsQuery, Result<List<ChatDto>>>
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

        public async Task<Result<List<ChatDto>>> Handle(GetChatsQuery request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.UserId))
            {
                return Error.Unauthorized("Unauthorized", "User is not authorized.");
            }

            var conversationsResult = await _conversationService.GetAllAsync(request.UserId);
            if (!conversationsResult.IsSuccess)
                return conversationsResult.Error!;

            var conversations = conversationsResult.Value;

            if (conversations.Count == 0)
                return new List<ChatDto>();

            var friendsResult = await _friendshipService.GetForUserAsync(request.UserId);
            if (!friendsResult.IsSuccess)
                return friendsResult.Error!;

            var friends = friendsResult.Value;

            if (friends.Count == 0)
                return new List<ChatDto>();

            var friendIds = friends.Select(f => f.Id).ToList();

            throw new Exception(); // refactor: after create new generic repository
        }
    }
}
