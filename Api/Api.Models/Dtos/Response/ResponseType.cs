using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Responses.Interfaces
{
    public enum ResponseType
    {
        InternalServerError,
        BadRequest,
        Unauthorized,
        NotFound,
        Conflict,
        Success
    }
}
