namespace ChatCommunicator.Contracts.Dtos.Chat
{
    public class PagedMessagesDto
    {
        public required IEnumerable<MessageDto> Messages { get; set; }
        public Guid? LastMessageReadId { get; set; }
    }
}
