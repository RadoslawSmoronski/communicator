namespace Application.DTOs
{
    public class AcceptFriendshipInviteDto
    {
        public required Guid FriendshipId { get; set; }
        public required Guid ConversationId { get; set; }
    }
}
