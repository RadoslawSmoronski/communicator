namespace ChatCommunicator.Contracts.Dtos.Chat
{
    public class ExtendedPagedMessagesDto
    {
        public required PagedMessagesDto PagedMessagesDto { get; set; }
        public List<string>? RecipientConnectionsId { get; set; }
    }
}
