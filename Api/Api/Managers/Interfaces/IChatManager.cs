using Api.Models.Chat;
using Api.Models.Dtos.Chat;
using Api.Utilities.Result;

namespace Api.Managers.Interfaces
{
    public interface IChatManager
    {
        //Conversation
        Task<ResultT<Conversation>> GetOrCreateConversationAsync(Guid userId, Guid friendId);
        Task<Result> DeleteConversationAsync(Guid conversationId);

        Task<ResultT<List<ChatDto>>> GetChatsAsync(Guid userId);

        //Message
        Task<Result> SaveMessageAsync(Message message);
        Task<ResultT<List<MessageDto>>> GetPagedMessagesFromMessageIdAsync(Guid conversationId, Guid fromMessageId);
    }
}
