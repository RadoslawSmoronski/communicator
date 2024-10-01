using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Exceptions.FriendshipInvitationRepository
{
    public class FriendshipInvitationIsAlreadyExistException : Exception
    {
        public FriendshipInvitationIsAlreadyExistException()
            : base("This friendship invitation already exists.")
        {
            
        }
    }
}
