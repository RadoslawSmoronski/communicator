namespace ChatCommunicator.Infrastructure.Models.Friendship
{
    public class FriendshipInvitation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public required Guid SenderId { get; set; }
        public required Guid RecipientId { get; set; }

        public required UserAccount SenderUser { get; set; }
        public required UserAccount RecipientUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
