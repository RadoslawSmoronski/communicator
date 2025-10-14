using Application.DTOs;
using Domain.Entities;
using Shared.Result;

namespace Application.Interfaces
{
    public interface IFriendshipService
    {
        Task<Result<Guid>> AddAsync(Guid user1Id, Guid user2Id);
        Task<Result<List<Friendship>>> GetAsync(Guid userId);
        //Task<ResultT<List<string>>> GetUserOnlineFriendsConnectionsIdAsync(Guid userId); // refactor: after signalR etc.
        Task<Result> DeleteAsync(Guid friendshipId);
    }
}
