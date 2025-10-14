using Infrastructure.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Identity
{
    [Table("Friendships")]
    public class FriendshipEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid User1Id { get; set; }
        public required Guid User2Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public required UserAccount User1 { get; set; }
        public required UserAccount User2 { get; set; }
    }
}
