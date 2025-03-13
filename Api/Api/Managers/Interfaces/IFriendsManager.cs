using Api.Models.Dtos;
using Api.Models.Dtos.Controllers.FriendsController;
using Api.Utilities.Result;

namespace Api.Managers.Interfaces
{
    public interface IFriendsManager
    {
        Task<Result> SendInviteAsync(string senderId, string recipientId);
        Task<ResultT<List<SimpleUserDto>>> GetInvitationsAsync(string userId);
        Task<Result> DecelineInviteAsync(string senderId, string recipientId);
        Task<Result> AddFriendsAsync(string senderId, string recipientId);
        Task<ResultT<List<SimpleUserDto>>> GetFriendsAsync(string userId);
        Task<bool> IsFriendsExistAsync(string userId1, string userId2);
        //Task<ResultT<List<UserForFriendInviteDto>>> GetUsersForFriendInviteByTextAsync(string userId, string text);
    }
}
