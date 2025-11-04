namespace Application.Users.Queries.GetChats
{
    public sealed record GetChatsReadModel(
        Guid FriendId,
        string FriendUserName,
        Guid ConversationId,
        string? FriendAvatarUrl = null,
        Guid? FriendshipId = null,
        Guid? LastMessageId = null,
        string? LastMessageContent = null,
        bool IsFriendSenderMessage = false,
        DateTime? LastMessageTimestamp = null
    );
}
