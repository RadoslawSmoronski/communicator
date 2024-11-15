using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Friendship
{
    public class FriendshipInvitation
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string SenderId { get; set; } = string.Empty;
        public string RecipientId { get; set; } = string.Empty;

        public UserAccount SenderUser { get; set; } = new UserAccount();
        public UserAccount RecipientUser { get; set; } = new UserAccount();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
