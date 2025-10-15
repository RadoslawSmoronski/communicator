using Domain.Entities;

namespace Application.Repositories
{
    public interface IConversationRepository : IBaseRepository<Conversation>
    {
        Task<Conversation?> GetConversationAsync(Guid user1Id, Guid user2Id);
        List<Conversation> GetUserAll(Guid userId);
    }
}
