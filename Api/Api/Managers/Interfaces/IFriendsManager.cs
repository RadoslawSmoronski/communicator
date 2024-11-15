using Api.Models.Dtos.Controllers.FriendsController;
using Api.Utilities.Result;

namespace Api.Managers.Interfaces
{
    public interface IFriendsManager
    {
        Task<Result> SendInviteAsync(string senderId, string recipientId);
        Task<ResultT<List<GetInvitiesUserDto>>> GetInvitiesAsync(string userId);
    }
}
