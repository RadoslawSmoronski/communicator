namespace ChatCommunicator.Contracts.Dtos.Chat
{
    public class GetPagedMessagesDto
    {
        public required Guid ConversationId { get; set; }
        public required Guid FromMessageId { get; set; }
    }
}
