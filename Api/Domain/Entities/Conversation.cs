namespace Domain.Entities
{
    public class Conversation : BaseEntity
    {
        public required Guid User1Id { get; set; }
        public required Guid User2Id { get; set; }
        public User? User1 { get; set; }
        public User? User2 { get; set; }

        public Guid? LastMessageId { get; set; }
        public Message? LastMessage { get; set; }
        public Guid? User1LastReadMessageId { get; set; }
        public Guid? User2LastReadMessageId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMessageTime { get; set; }
    }
}
