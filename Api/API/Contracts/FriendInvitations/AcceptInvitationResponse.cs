namespace API.Contracts.FriendInvitations
{
    public sealed record AcceptInvitationResponse(
        Guid FriendshipId,
        Guid ConversationId
    );
}
