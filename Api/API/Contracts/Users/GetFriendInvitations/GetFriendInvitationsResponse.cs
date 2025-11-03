namespace API.Contracts.Users.GetFriendInvitations
{
    public sealed record GetFriendshipInvitationResponse(
        Guid FriendInvitationId,
        Guid SenderId,
        string SenderUsername,
        string? SenderAvatarUrl
        );
}
