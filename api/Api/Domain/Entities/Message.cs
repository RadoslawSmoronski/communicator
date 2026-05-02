namespace Domain.Entities
{
    public class Message : BaseEntity
    {
        public required Guid ConversationId { get; set; }
        public Conversation? Conversation { get; set; }
        public required Guid SenderId { get; set; }
        public User? Sender { get; set; }
        public required string Content { get; set; }
    }
}