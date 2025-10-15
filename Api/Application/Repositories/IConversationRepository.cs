using Domain.Entities;

namespace Application.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation?> GetConversationAsync(Guid user1Id, Guid user2Id);
        Task AddAsync(Conversation conversation);
        Task<List<Conversation>> GetUserAllAsync(Guid userId);
    }
}
