using Api.Models.Dtos.Response.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Response
{
    public class SuccessResponseDto : ISuccessResponseDto
    {
        public string Type { get; } = "https://tools.ietf.org/html/rfc9110#section-15.3.1";
        public string Title { get; set; } = String.Empty;
        public int Status { get; } = 200;
        public string TraceId { get; set; } = String.Empty;
    }
}
