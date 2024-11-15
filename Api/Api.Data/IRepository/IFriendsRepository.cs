using Api.Models;
using Api.Utilities.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Data.IRepository
{
    public interface IFriendsRepository
    {
        Task<bool> IsFriendsInvitationExists(string userId1, string userId2);
        Task SendInviteAsync(UserAccount senderUser, UserAccount recipientUser);
    }
}
