namespace Application.Chats.Queries.GetPagedMessages
{
    public sealed record MessageReadModel(
        Guid MessageId,
        Guid ConversationId,
        Guid SenderId,
        string Content,
        DateTime Timestamp
    );
}
