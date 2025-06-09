using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Models.Dtos.Chat
{
    public class GetPagedMessagesDto
    {
        public Guid ConversationId { get; set; }
        public Guid FromMessageId { get; set; }
    }
}
