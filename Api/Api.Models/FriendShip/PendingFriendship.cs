using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.FriendShip
{
    public class PendingFriendship
    {
        public int Id { get; set; }

        public string User1Id { get; set; } = string.Empty;
        public string User2Id { get; set; } = string.Empty;

        public UserAccount User1 { get; set; } = new UserAccount();
        public UserAccount User2 { get; set; } = new UserAccount();

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
