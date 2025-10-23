using Domain.Entities;

namespace Application.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation?> GetConversationByUsersIdAsync(Guid user1Id, Guid user2Id);
        Task<Conversation?> GetConversationByIdAsync(Guid conversationId);
        List<Conversation> GetUserAll(Guid userId);

        Task AddAsync(Conversation entity);
        Task DeleteAsync(Guid id);
        Task UpdateAsync(Conversation entity);
    }
}
