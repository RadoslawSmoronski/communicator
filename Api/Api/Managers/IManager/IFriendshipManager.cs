using Api.Models.Dtos.Managers.FriendshipManager.SendInviteAsync;

namespace Api.Managers.IManager
{
    public interface IFriendshipManager
    {
        Task<SendInviteResultDto> SendInviteAsync(string senderId, string RecipientId);
    }
}
