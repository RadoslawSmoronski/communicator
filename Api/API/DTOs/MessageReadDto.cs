namespace API.DTOs
{
    public class MessageReadDto
    {
        public required Guid MessageId { get; set; }
        public required Guid ConversationId { get; set; }
    }
}
