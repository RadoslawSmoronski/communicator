using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Entities
{
    [Table("FriendshipInvitations")]
    public class FriendshipInvitationEntity : BaseEntityInfrastructure
    {
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        [ForeignKey(nameof(SenderId))]
        public UserAccount? SenderUser { get; set; }
        [ForeignKey(nameof(RecipientId))]
        public UserAccount? RecipientUser { get; set; }
    } 
}
