using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Contracts.Dtos.Controllers.FriendsController
{
    public class UserToInviteDto
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string UserName { get; set; } = String.Empty;
        public bool IsInvited { get; set; } = false;
    }
}
