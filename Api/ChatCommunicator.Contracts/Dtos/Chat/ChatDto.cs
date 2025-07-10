using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Contracts.Dtos.Chat
{
    public class ChatDto
    {
        public Guid FriendId { get; set; } = Guid.Empty;
        public string FriendUserName {  get; set; } = String.Empty;
        public Guid ConversationId { get; set; } = Guid.Empty;
        public Guid? LastMessageId { get; set; }
        public string? LastMessageContent { get; set; }
        public bool IsFriendSenderMessage { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }
    }
}