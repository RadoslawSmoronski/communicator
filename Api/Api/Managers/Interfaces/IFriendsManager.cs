using ChatCommunicator.Models.Dtos;
using ChatCommunicator.Models.Dtos.Controllers.FriendsController;
using ChatCommunicator.Shared.Result;

namespace ChatCommunicator.Managers.Interfaces
{
    public interface IFriendsManager
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
