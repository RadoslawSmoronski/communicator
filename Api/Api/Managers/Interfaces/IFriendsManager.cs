using Api.Models.Dtos.Controllers.FriendsController;
using Api.Utilities.Result;

namespace Api.Managers.Interfaces
{
    public interface IFriendsManager
    {
        Task<Result> SendInviteAsync(string senderId, string recipientId);
        Task<ResultT<List<GetInvitationsUserDto>>> GetInvitationsAsync(string userId);
        Task<Result> DecelineInviteAsync(string senderId, string recipientId);
        Task<Result> AddFriendsAsync(string senderId, string recipientId);
        Task<ResultT<List<FriendDto>>> GetFriendsAsync(string userId);
        Task<ResultT<List<UserForFriendInviteDto>>> GetUsersForFriendInviteByTextAsync(string userId, string text);
    }
}
