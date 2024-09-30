using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException()
            : base("Not found this user.")
        {
        }

        public UserNotFoundException(string message)
            : base(message)
        {
        }
    }
}
