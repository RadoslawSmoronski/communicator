using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Chat
{
    public class ChatDto
    {
        public string FriendId { get; set; } = String.Empty;
        public string FriendUserName {  get; set; } = String.Empty;
        public string ConversationId { get; set; } = String.Empty;
        public string? LastMessageId { get; set; }
        public string? LastMessageContent { get; set; }
        public bool IsFriendSenderMessage { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }
    }
}