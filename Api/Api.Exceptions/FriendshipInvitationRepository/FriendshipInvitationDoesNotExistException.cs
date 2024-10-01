using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Exceptions.FriendshipInvitationRepository
{
    public class FriendshipInvitationDoesNotExistException : Exception
    {
        public FriendshipInvitationDoesNotExistException()
            : base("User has no invitations sent or received.")
        {
            
        }
    }
}
