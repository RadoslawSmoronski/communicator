using Domain.Entities;
using Infrastructure.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Identity
{
    [Table("Friendships")]
    public class FriendshipEntity : BaseEntity
    {
        public required Guid User1Id { get; set; }
        public required Guid User2Id { get; set; }
        [ForeignKey(nameof(User1Id))]
        public required UserAccount User1 { get; set; }
        [ForeignKey(nameof(User2Id))]
        public required UserAccount User2 { get; set; }
    }
}
