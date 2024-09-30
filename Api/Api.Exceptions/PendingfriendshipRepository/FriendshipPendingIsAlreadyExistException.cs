using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Exceptions.PendingfriendshipRepository
{
    public class FriendshipPendingIsAlreadyExistException : Exception
    {
        public FriendshipPendingIsAlreadyExistException()
            : base("This friendship invitation already exists.")
        {
            
        }
    }
}
