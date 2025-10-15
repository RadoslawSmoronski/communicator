using Application.DTOs;
using Domain.Entities;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IConversationService
    {
        Task<Result<Conversation>> GetOrCreateAsync(Guid userId, Guid friendId);
        Result<List<Conversation>> GetAll(Guid userId);
    }
}
