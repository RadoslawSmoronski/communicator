using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Exceptions.Friendship
{
    public class FriendshipDoesNotExistException : Exception
    {
        public FriendshipDoesNotExistException()
            : base("User has no friends.")
        {
            
        }
    }
}
