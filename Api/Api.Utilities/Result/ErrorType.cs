using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Utilities.Result
{
    public enum ErrorType
    {
        Unknown,
        Validation,
        Unauthorized,
        Forbidden,
        NotFound,
        Conflict,
        Failure
    }
}
