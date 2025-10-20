using Domain.Entities;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IFriendshipService
    {
        Task<Result<Guid>> AddAsync(Guid user1Id, Guid user2Id);
        Task<Result<List<Friend>>> GetUserFriendAsync(Guid userId);
        Task<Result> DeleteAsync(Guid friendshipId);
        Task<Result> IsExistAsync(Guid user1Id, Guid user2Id);
    }
}
