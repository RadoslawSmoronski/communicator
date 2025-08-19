namespace ChatCommunicator.Contracts.Dtos.Chat
{
    public class ChatDto
    {
        public required Guid FriendId { get; set; }
        public required string FriendUserName {  get; set; }
        public string? FriendAvatarUrl { get; set; }
        public bool IsFriendOnline { get; set; } = false;
        public required Guid ConversationId { get; set; }
        public Guid? LastMessageId { get; set; }
        public string? LastMessageContent { get; set; }
        public bool IsFriendSenderMessage { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }
    }
}