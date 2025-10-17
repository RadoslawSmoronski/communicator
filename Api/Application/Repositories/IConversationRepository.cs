using Domain.Entities;

namespace Application.Repositories
{
    public interface IConversationRepository : IBaseRepository<Conversation>
    {
        Task<Conversation?> GetConversationByUsersIdAsync(Guid user1Id, Guid user2Id);
        Task<Conversation?> GetConversationByIdAsync(Guid conversationId);
        List<Conversation> GetUserAll(Guid userId);
    }
}
