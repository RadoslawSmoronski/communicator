using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Responses.Interfaces
{
    public interface IErrorResponseWithErrorsDto
    {
        IDictionary<string, IEnumerable<string>> Errors { get; set; }
    }
}