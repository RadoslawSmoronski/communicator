namespace Application.DTOs
{
    public class ExtendedPagedMessagesDto
    {
        public required PagedMessagesDto PagedMessagesDto { get; set; }
        public List<string>? RecipientConnectionsId { get; set; }
        public Guid? UserReadMessageId { get; set; }
    }
}
