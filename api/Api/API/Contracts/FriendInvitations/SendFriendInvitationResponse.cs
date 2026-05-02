namespace API.Contracts.FriendInvitations
{
    public sealed record SendFriendInvitationResponse(
        Guid FriendshipInvitationId
    );
}
