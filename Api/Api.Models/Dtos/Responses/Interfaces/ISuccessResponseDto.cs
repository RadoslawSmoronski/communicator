using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Responses.Interfaces
{
    public interface ISuccessResponseDto
    {
        string Type { get; }
        string Title { get; set; }
        int Status { get; }
        string TraceId { get; set; }
    }
}
