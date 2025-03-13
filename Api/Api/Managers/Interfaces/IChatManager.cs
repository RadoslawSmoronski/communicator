using Api.Models.Chat;
using Api.Utilities.Result;

namespace Api.Managers.Interfaces
{
    public interface IChatManager
    {
        //Conversation
        ResultT<Conversation> GetOrCreateConversation(string friendId);

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
