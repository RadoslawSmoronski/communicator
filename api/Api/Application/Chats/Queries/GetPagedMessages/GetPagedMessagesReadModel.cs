namespace Application.Chats.Queries.GetPagedMessages
{
    public sealed record GetPagedMessagesReadModel(
        IReadOnlyList<MessageReadModel> Messages,
        Guid? LastFriendReadMessageId,
        IReadOnlyList<string>? RecipientConnectionsId,
        Guid? UserReadMessageId
    );
}
