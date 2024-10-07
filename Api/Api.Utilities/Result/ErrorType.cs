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
        AccessUnAuthorized = 4,
        AccessForbidden = 5
    }
}
