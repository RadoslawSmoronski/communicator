using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Models.Friendship
{
    public class Friendship
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid User1Id { get; set; } = Guid.Empty;
        public Guid User2Id { get; set; } = Guid.Empty;

        public UserAccount User1 { get; set; } = new UserAccount();
        public UserAccount User2 { get; set; } = new UserAccount();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
