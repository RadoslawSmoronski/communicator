namespace ChatCommunicator.Contracts.Dtos
{
    public class MessageReadDto
    {
        public required Guid FriendId { get; set; }
        public required Guid ConversationId { get; set; }
    }
}
