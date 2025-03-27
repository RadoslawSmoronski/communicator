using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.FriendsController
{
    public class UserToInviteDto
    {
        public string Id { get; set; } = String.Empty;
        public string userName { get; set; } = String.Empty;
        public bool IsInvited { get; set; } = false;
    }
}
