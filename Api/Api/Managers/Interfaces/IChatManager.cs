using Api.Models.Chat;
using Api.Utilities.Result;

namespace Api.Managers.Interfaces
{
    public interface IChatManager
    {
        //Conversation
        Task<ResultT<Conversation>> GetOrCreateConversationAsync(string userId, string friendId);
        Task<Result> DeleteConversationAsync(string conversationId);

        //Message
        //ResultT<bool> SendMessage(string conversationId, string message);
        //ResultT<List<Message>> GetMessages(string conversationId, int amount, string? beforeMessageId);
        //ResultT<List<Message>> GetUnreadMessages();
        //ResultT<Message> GetLastMessage();

        ////GetUserStatus(string userId);
        ////Result MarkMessagesAsRead(Guid conversationId, List<Guid> messageIds);
        ////ResultT<List<Conversation>> GetConversations(int amount, int offset);
    }
}
