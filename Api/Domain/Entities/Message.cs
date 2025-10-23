namespace Domain.Entities
{
    public class Message : BaseEntity
    {
        public required Guid ConversationId { get; set; }
        public required Guid SenderId { get; set; }
        public required User Sender { get; set; }
        public required string Content { get; set; }
        public required DateTime Timestamp { get; set; }
    }
}