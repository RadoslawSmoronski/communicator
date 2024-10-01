using Api.Models.Friendship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.FriendsController
{
    public class GetInvitiesOkResponseDto
    {
        public bool Succeeded { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public List<FriendshipInvitation> FriendshipInvitations { get; set; } = new List<FriendshipInvitation>();
    }
}
