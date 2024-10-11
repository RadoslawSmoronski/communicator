using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Utilities.Result
{
    public enum ErrorType
    {
        NotFound = 404,
        Validation = 400,
        Conflict = 409,
        AccessUnauthorized = 401,
        InternalServerError = 500
    }
}
