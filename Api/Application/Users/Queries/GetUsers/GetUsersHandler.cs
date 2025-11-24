using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Users.Queries.GetUsers
{
    public class GetUsersHandler(
        IUserService userService,
        IFriendshipService friendshipService,
        IFriendInvitationsService friendInvitationsService,
        IUserAvatarService avatarService)
        : IRequestHandler<GetUsersQuery, Result<List<GetUsersReadModel>>>
    {
        private readonly IUserService _userService = userService;
        private readonly IFriendshipService _friendshipService = friendshipService;
        private readonly IFriendInvitationsService _friendInvitationsService = friendInvitationsService;
        private readonly IUserAvatarService _avatarService = avatarService;

        public async Task<Result<List<GetUsersReadModel>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            if (request.CanBeInvitedByUserId is null || request.CanBeInvitedByUserId.Value == Guid.Empty)
            {
                return Error.Validation(
                    "FriendInvitations.GetUsers.InvalidCanBeInvitedByUserId",
                    "The 'CanBeInvitedByUserId' user id is required and cannot be an empty GUID."
                );
            }

            var canBeInvitedByUserId = request.CanBeInvitedByUserId.Value;

            var usersResult = await _userService.GetAllAsync();
            if (!usersResult.IsSuccess)
                return usersResult.Error!;

            var users = usersResult.Value;
            if (users.Count < 1)
                return new List<GetUsersReadModel>();


            var userFriendsResult = await _friendshipService.GetUserFriendAsync(canBeInvitedByUserId);
            if (!userFriendsResult.IsSuccess)
                return userFriendsResult.Error!;

            var userFriends = userFriendsResult.Value;
            var usersFriendsIds = new HashSet<Guid>(userFriends.Select(x => x.Id));

            var userInvitationsResult = await _friendInvitationsService.GetUserInvitations(canBeInvitedByUserId);
            if (!userInvitationsResult.IsSuccess)
                return userInvitationsResult.Error!;

            var userInvitations = userInvitationsResult.Value;
            var userInvitationsIds = new HashSet<Guid>(userInvitations.Select(x => x.SenderId == canBeInvitedByUserId ? x.RecipientId : x.SenderId));

            var addableUsers = users
                .Where(u => u.Id != canBeInvitedByUserId && !usersFriendsIds.Contains(u.Id))
                .Select(x => new GetUsersReadModel(
                    Id: x.Id,
                    AvatarUrl: x.AvatarUrl is not null ? _avatarService.GetPublicAvatarUrl(x.AvatarUrl) : null,
                    UserName: x.UserName,
                    IsInvited: userInvitationsIds.Contains(x.Id)))
                .ToList();

            return addableUsers;
        }
    }
}
