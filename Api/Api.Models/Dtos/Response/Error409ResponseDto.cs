﻿using Api.Models.Dtos.Response.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Response
{
    public class Error409ResponseDto : IErrorResponseDto
    {
        public string Type { get; } = "https://tools.ietf.org/html/rfc9110#section-15.5.10";
        public string Title { get; set; } = String.Empty;
        public int Status { get; } = 409;
        public IDictionary<string, IEnumerable<string>> Errors { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public string TraceId { get; set; } = String.Empty;
    }
}
