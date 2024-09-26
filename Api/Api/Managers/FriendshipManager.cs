using Api.Managers.IManager;
using Api.Models.Dtos.Managers.FriendshipManager.SendInviteAsync;

namespace Api.Managers
{
    public class FriendshipManager : IFriendshipManager
    {
        public async Task<SendInviteResultDto> SendInviteAsync(string senderId, string recipientId)
        {
            var result = new SendInviteResultDto
            {
                Succeeded = true
            };

            return result;
        }
    }
}
