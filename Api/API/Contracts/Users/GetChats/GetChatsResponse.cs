namespace API.Contracts.Users.GetChats
{
    public sealed record GetChatsResponse(
        Guid FriendId,
        string FriendUsername,
        Guid ConversationId,
        string? FriendAvatarUrl = null,
        Guid? FriendshipId = null,
        Guid? LastMessageId = null,
        string? LastMessageContent = null,
        bool IsFriendSenderMessage = false,
        DateTime? LastMessageTimestamp = null
    );
}
