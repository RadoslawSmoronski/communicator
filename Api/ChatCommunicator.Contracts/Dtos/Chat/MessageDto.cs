using ChatCommunicator.Contracts.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Contracts.Dtos.Chat
{
    public class MessageDto
    {
        public String MessageId { get; set; } = string.Empty;
        public String ConversationId { get; set; } = string.Empty;

        public string SenderId { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
