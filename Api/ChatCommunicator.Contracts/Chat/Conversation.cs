using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Contracts.Chat
{
    public class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid User1Id { get; set; } = Guid.Empty;
        public Guid User2Id { get; set; } = Guid.Empty;

        public UserAccount? User1 { get; set; }
        public UserAccount? User2 { get; set; }

        public Guid? LastMessageId { get; set; }
        public Message? LastMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMessageTime { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
