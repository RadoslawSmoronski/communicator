using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Exceptions
{
    public class DatabaseOperationException : Exception
    {
        public DatabaseOperationException()
            : base("Database operation error.")
        {
        }

        public DatabaseOperationException(string message)
            : base(message)
        {
        }
    }
}
