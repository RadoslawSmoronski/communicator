namespace Application.FriendInvitations.Commands.AcceptFriendInvtation
{
    public sealed record AcceptFriendInvitationReadModel(
        Guid FriendshipId,
        Guid ConversationId
    );
}
