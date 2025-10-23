namespace Application.DTOs
{
    public class PagedMessagesDto
    {
        public required IEnumerable<MessageDto> Messages { get; set; }
        public Guid? LastFriendReadMessageId { get; set; }
    }
}
