namespace ChatCommunicator.Infrastructure.Models.Chat
{
    public class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid User1Id { get; set; }
        public required Guid User2Id { get; set; }

        public required UserAccount User1 { get; set; }
        public required UserAccount User2 { get; set; }

        public Guid? LastMessageId { get; set; }
        public Message? LastMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMessageTime { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
