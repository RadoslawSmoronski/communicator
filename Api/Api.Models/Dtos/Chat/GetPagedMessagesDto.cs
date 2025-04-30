using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Chat
{
    public class GetPagedMessagesDto
    {
        public string ConversationId { get; set; }
        public string FromMessageId { get; set; }
    }
}
