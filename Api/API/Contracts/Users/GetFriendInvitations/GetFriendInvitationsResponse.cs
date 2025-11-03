namespace API.Contracts.Users.GetFriendInvitations
{
    public sealed record FriendshipInvitationResponse(
        Guid FriendInvitationId,
        Guid SenderId,
        string SenderUserName,
        string? SenderAvatarUrl
        );
}
