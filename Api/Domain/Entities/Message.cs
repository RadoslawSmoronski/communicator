namespace Domain.Entities
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid ConversationId { get; set; }
        public required Conversation Conversation { get; set; }
        public required Guid SenderId { get; set; }
        public required string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}