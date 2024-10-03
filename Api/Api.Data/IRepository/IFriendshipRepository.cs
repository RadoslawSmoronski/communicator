using Api.Models.Dtos.Controllers.FriendsController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Api.Data.IRepository
{
    public interface IFriendshipRepository
    {
        Task AddFriendshipAsync(string user1Id, string user2Id);
        Task<List<FriendDto>> GetFriendsAsync(string userId);
        Task<bool> IsFriendshipExistsAsync(string user1Id, string user2Id);
    }
}
