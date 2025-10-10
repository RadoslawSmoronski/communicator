using Infrastructure.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Identity
{
    [Table("FriendshipInvitations")]
    public class FriendshipInvitationEntity
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserAccount SenderUser { get; set; } = null!;
        public UserAccount RecipientUser { get; set; } = null!;
    }
}
