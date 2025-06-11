using ChatCommunicator.Contracts.Chat;
using ChatCommunicator.Contracts.Dtos.Chat;
using ChatCommunicator.Shared.Result;

namespace ChatCommunicator.Application.Services.Interfaces
{
    public interface IChatService
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
