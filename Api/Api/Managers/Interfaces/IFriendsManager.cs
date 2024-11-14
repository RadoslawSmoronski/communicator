using Api.Utilities.Result;

namespace Api.Managers.Interfaces
{
    public interface IFriendsManager
    {
        Task<Result> SendInviteAsync(string SenderId, string RecipientId);
    }
}
