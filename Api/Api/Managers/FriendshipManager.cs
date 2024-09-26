using Api.Data.IRepository;
using Api.Managers.IManager;
using Api.Models.Dtos.Managers.FriendshipManager.SendInviteAsync;

namespace Api.Managers
{
    public class FriendshipManager : IFriendshipManager
    {
        private readonly IPendingFriendshipRepository _pendingFriendshipRepository;

        public FriendshipManager(IPendingFriendshipRepository pendingFriendshipRepository)
        {
            _pendingFriendshipRepository = pendingFriendshipRepository;
        }

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
