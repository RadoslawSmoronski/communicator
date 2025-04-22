using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Chat
{
    public class GetMessagesDto
    {
        public string ConversationId { get; set; }
        public int PageNumber { get; set; }
    }
}
