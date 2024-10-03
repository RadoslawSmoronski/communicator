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
        Task<List<FriendsInvitationDto>> GetInvitiesAsync(string userId);
        Task DeleteInviteAsync(string senderId, string recipientId);
        Task<bool> IsFriendshipInvitationExists(string user1, string user2);
    }
}
