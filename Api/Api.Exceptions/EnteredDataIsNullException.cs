using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Exceptions
{
    public class EnteredDataIsNullException : Exception
    {
        public EnteredDataIsNullException()
            : base("Entered data is null.")
        {
        }

        public EnteredDataIsNullException(string message)
            : base(message)
        {
        }
    }
}
