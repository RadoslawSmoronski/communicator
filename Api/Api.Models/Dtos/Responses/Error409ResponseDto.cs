using Api.Models.Dtos.Responses.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Responses
{
    public class Error409ResponseDto : IResponse
    {
        public string Type { get; } = "https://tools.ietf.org/html/rfc9110#section-15.5.10";
        public string Title { get; set; } = String.Empty;
        public int Status { get; } = 409;
        public string? TraceId { get; set; }
    }
}