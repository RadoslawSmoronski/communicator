namespace ChatCommunicator.Contracts.Chat
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid ConversationId { get; set; }
        public required Conversation Conversation { get; set; }
        
        public required Guid SenderId { get; set; }
        public required UserAccount Sender { get; set; }

        public required string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}