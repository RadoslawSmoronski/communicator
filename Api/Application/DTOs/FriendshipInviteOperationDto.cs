namespace Application.DTOs
{
    public class FriendshipInviteOperationDto
    {
        public required Guid SenderId { get; set; }
        public required Guid RecipientId { get; set; }
    }
}
