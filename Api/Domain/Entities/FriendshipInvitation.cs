namespace Domain.Entities
{
    public class FriendshipInvitation : BaseEntity
    {
        public required Guid SenderId { get; set; }
        public required Guid RecipientId { get; set; }
        public User? SenderUser { get; set; }
        public User? RecipientUser { get; set; }

    }
}
