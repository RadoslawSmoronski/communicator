using ChatCommunicator.Contracts.Dtos.Chat;
using ChatCommunicator.Infrastructure.Models.Chat;
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
        Task<ResultT<PagedMessagesDto>> GetPagedMessagesFromMessageIdAsync(Guid? conversationId, Guid? fromMessageId, Guid? userId);
    }
}
