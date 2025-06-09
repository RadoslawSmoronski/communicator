using ChatCommunicator.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Data.Repository.Interfaces
{
    public interface IMessageRepository : IRepository<Message>
    {
        Task<IEnumerable<Message>> GetPagedMessagesFromMessageIdAsync(Guid ConversationId, Guid fromMessageId, int pageSize);
    }
}
