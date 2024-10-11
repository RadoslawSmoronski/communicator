using Api.Models.Dtos.Responses.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Responses
{
    public class Error401ResponseWithErrorsDto : IResponse, IResponseWithErrors
    {
        public string Type { get; } = "https://tools.ietf.org/html/rfc9110#section-15.5.2";
        public string Title { get; set; } = String.Empty;
        public int Status { get; } = 401;
        public IDictionary<string, IEnumerable<string>> Errors { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public string? TraceId { get; set; } = Activity.Current?.Id;
    }
}
