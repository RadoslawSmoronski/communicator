using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Contracts.Friendship
{
    public class FriendshipInvitation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid SenderId { get; set; } = Guid.Empty;
        public Guid RecipientId { get; set; } = Guid.Empty;

        public UserAccount SenderUser { get; set; } = new UserAccount();
        public UserAccount RecipientUser { get; set; } = new UserAccount();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
