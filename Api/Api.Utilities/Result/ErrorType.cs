using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Utilities.Result
{
    public enum ErrorType
    {
        Failure = 0,
        NotFound = 1,
        Validation = 2,
        Conflict = 3,
        AccessUnauthorized = 4,
        InternalServerError = 5
    }
}
