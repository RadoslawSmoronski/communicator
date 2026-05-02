namespace Application.Contracts.Chat
{
    public record MessageReceivedEvent(
        Guid MessageId,
        Guid ConversationId,
        Guid SenderId,
        string Content,
        DateTime Timestamp
        );
}
