namespace Application.Users.Queries.GetInvitations
{
    public sealed record GetInvitationsReadModel(
        Guid FriendInvitationId,
        Guid SenderId,
        string SenderUserName,
        string? SenderAvatarUrl = null
    );
}
