namespace Application.Contracts.Chat
{
    public record MessageReadEvent(
        Guid MessageId,
        Guid ConversationId
        );
}
