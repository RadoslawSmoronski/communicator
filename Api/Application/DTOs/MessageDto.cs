namespace Application.DTOs
{
    public class MessageDto
    {
        public required Guid MessageId { get; set; }
        public required Guid ConversationId { get; set; }

        public required Guid SenderId { get; set; }

        public required string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
