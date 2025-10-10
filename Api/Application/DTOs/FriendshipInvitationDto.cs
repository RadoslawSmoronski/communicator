namespace Application.DTOs
{
    public class FriendshipInvitationDto
    {
        public required Guid FriendInvitationId { get; set; }
        public required Guid SenderId { get; set; }
        public required string SenderUserName { get; set; }
        public string? SenderAvatarUrl { get; set; }
    }
}
