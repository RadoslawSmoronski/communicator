namespace Application.DTOs
{
    public class AcceptFriendshipInviteDto
    {
        public Guid? FriendshipId { get; set; } //refactor
        public required Guid? ConversationId { get; set; }
    }
}
