namespace ChatCommunicator.Contracts.Dtos
{
    public class MessageReadDto
    {
        public required Guid MessageId { get; set; }
        public required Guid ConversationId { get; set; }
    }
}
