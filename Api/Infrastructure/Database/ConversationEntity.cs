using Infrastructure.Services;

namespace Infrastructure.Database
{
    public class ConversationEntity
    {
        public required Guid Id { get; set; } = Guid.NewGuid();
        public required Guid User1Id { get; set; }
        public required Guid User2Id { get; set; }
        public UserAccount? User1 { get; set; }
        public UserAccount? User2 { get; set; }
        public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
