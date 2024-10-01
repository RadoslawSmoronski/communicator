using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.FriendsController
{
    public class FriendsInvitationDto
    {
        public string SenderId { get; set; } = String.Empty;
        public string SenderUsername { get; set; } = String.Empty;
    }
}
