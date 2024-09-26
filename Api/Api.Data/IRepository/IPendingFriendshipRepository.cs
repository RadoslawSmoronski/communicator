using Api.Models.Friendship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Data.IRepository
{
    public interface IPendingFriendshipRepository
    {
        Task SendInviteAsync(string senderId, string recipientId);
        Task<List<PendingFriendship>> CheckInvitiesAsync(string recipientId);
        Task AcceptInviteAsync(string recipientId, string senderId);
        Task DecelineInviteAsync(string recipientId, string senderId);
        Task<bool> IsFriendshipPendingExists(string user1, string user2);
    }
}
