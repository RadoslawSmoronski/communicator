using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Exceptions.FriendshipRepository
{
    public class FriendshipIsAlreadyExistException : Exception
    {
        public FriendshipIsAlreadyExistException()
            : base("This friendship already exists.")
        {
            
        }
    }
}
