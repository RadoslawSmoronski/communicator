using Api.Models.Dtos.Responses.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Responses
{
    public class Error500ResponseDto : IErrorResponseDto, IErrorResponseWithErrorsDto
    {
        public string Type { get; } = "https://tools.ietf.org/html/rfc9110#section-15.6.1";
        public string Title { get; set; } = String.Empty;
        public int Status { get; } = 500;
        public IDictionary<string, IEnumerable<string>> Errors { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public string? TraceId { get; set; }
    }
}
