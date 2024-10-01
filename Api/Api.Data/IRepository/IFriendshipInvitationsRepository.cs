using Api.Models.Dtos.Controllers.FriendsController;
using Api.Models.Friendship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Data.IRepository
{
    public interface IFriendshipInvitationsRepository
    {
        Task SendInviteAsync(string senderId, string recipientId);
        Task<List<FriendsInvitationDto>> GetUserInvitiesAsync(string userId);
        Task AcceptInviteAsync(string recipientId, string senderId);
        Task DecelineInviteAsync(string recipientId, string senderId);
        Task<bool> IsFriendshipInvitationExists(string user1, string user2);
    }
}
