using Api.Models.Chat;
using Api.Models.Dtos.Chat;
using Api.Utilities.Result;

namespace Api.Managers.Interfaces
{
    public interface IChatManager
    {
        //Conversation
        Task<ResultT<Conversation>> GetOrCreateConversationAsync(string userId, string friendId);
        Task<Result> DeleteConversationAsync(string conversationId);

        Task<ResultT<List<ChatDto>>> GetChatsAsync(string userId);

        //Message
        Task<Result> SaveMessageAsync(Message message);
        Task<ResultT<List<MessageDto>>> GetPagedMessagesFromMessageIdAsync(string conversationId, string fromMessageId);
    }
}
