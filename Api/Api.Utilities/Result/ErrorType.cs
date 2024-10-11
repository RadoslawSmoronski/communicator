using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Utilities.Result
{
    public enum ErrorType
    {
        Validation = 0,
        Unauthorized = 1,
        NotFound = 2,
        Conflict = 3,
        InternalServerError = 4
    }
}
