namespace Domain.Entities
{
    public class Friendship : BaseEntity
    {
        public required Guid User1Id { get; set; }
        public required Guid User2Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public User? User1 { get; set; }
        public User? User2 { get; set; }
    }
}
