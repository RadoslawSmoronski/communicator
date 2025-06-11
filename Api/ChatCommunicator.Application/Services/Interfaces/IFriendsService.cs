using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Controllers.FriendsController;
using ChatCommunicator.Shared.Result;

namespace ChatCommunicator.Application.Services.Interfaces
{
    public interface IFriendsService
    {
        Task<Result> SendInviteAsync(Guid senderId, Guid recipientId);
        Task<ResultT<List<SimpleUserDto>>> GetInvitationsAsync(Guid userId);
        Task<ResultT<List<UserToInviteDto>>> GetUsersToInviteByTextAsync(Guid userId, string text);
        Task<Result> DecelineInviteAsync(Guid senderId, Guid recipientId);
        Task<Result> AddFriendsAsync(Guid senderId, Guid recipientId);
        Task<ResultT<List<SimpleUserDto>>> GetFriendsAsync(Guid userId);
        Task<bool> IsFriendsExistAsync(Guid userId1, Guid userId2);
    }
}
