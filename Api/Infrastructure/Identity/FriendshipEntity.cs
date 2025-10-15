using Domain.Entities;
using Infrastructure.Database;
using Infrastructure.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Identity
{
    [Table("Friendships")]
    public class FriendshipEntity : BaseEntity
    {
        public required Guid User1Id { get; set; }
        public required Guid User2Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public required UserAccount User1 { get; set; }
        public required UserAccount User2 { get; set; }
    }
}
