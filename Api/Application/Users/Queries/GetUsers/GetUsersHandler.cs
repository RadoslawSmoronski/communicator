using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Users.Queries.GetUsers
{
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, Result<List<UserToInviteDto>>>
    {
        private readonly IUserService _userService;
        private readonly IFriendshipService _friendshipService;
        private readonly IFriendInvitationsService _friendInvitationsService;

        public GetUsersHandler(IUserService userService, IFriendshipService friendshipService, IFriendInvitationsService friendInvitationsService)
        {
            _userService = userService;
            _friendshipService = friendshipService;
            _friendInvitationsService = friendInvitationsService;
        }

        public async Task<Result<List<UserToInviteDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            if (request.InvitableFor is null || request.InvitableFor.Value == Guid.Empty)
            {
                return Error.Validation(
                    "FriendInvitations.GetUsers.InvalidInvitableFor",
                    "The 'InvitableFor' user id is required and cannot be an empty GUID."
                );
            }

            var invitableForId = request.InvitableFor.Value;

            if (!_userService.IsAuthorized(invitableForId))
            {
                return Error.Unauthorized(
                    "FriendInvitations.GetUsers.Unauthorized",
                    "The current user is not authorized to get invitable users for the specified account."
                );
            }

            var usersResult = await _userService.GetAllAsync();
            if (!usersResult.IsSuccess)
                return usersResult.Error!;

            var users = usersResult.Value;
            if (users.Count < 1)
                return new List<UserToInviteDto>();


            var userFriendsResult = await _friendshipService.GetUserFriendAsync(invitableForId);
            if (!userFriendsResult.IsSuccess)
                return userFriendsResult.Error!;

            var userFriends = userFriendsResult.Value;
            var usersFriendsIds = new HashSet<Guid>(userFriends.Select(x => x.Id));

            var userInvitationsResult = await _friendInvitationsService.GetUserInvitations(invitableForId);
            if (!userInvitationsResult.IsSuccess)
                return userInvitationsResult.Error!;

            var userInvitations = userInvitationsResult.Value;
            var userInvitationsIds = new HashSet<Guid>(userInvitations.Select(x => x.SenderId == invitableForId ? x.RecipientId : x.SenderId));

            var addableUsers = users
                .Where(u => u.Id != invitableForId && !usersFriendsIds.Contains(u.Id))
                .Select(x => new UserToInviteDto
                {
                    Id = x.Id,
                    AvatarUrl = x.AvatarUrl,
                    UserName = x.UserName,
                    IsInvited = userInvitationsIds.Contains(x.Id)
                })
                .ToList();

            return addableUsers;
        }
    }
}
