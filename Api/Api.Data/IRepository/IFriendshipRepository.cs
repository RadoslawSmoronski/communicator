using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Data.IRepository
{
    public interface IFriendshipRepository
    {
        Task AddFriendshipAsync(string user1Id, string user2Id);
    }
}
