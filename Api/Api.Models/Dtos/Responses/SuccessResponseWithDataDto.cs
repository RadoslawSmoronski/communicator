﻿using Api.Models.Dtos.Responses.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Responses
{
    public class SuccessResponseWithDataDto<T> : ISuccessResponseDto
    {
        public string Type { get; } = "https://tools.ietf.org/html/rfc9110#section-15.3.1";
        public string Title { get; set; } = string.Empty;
        public int Status { get; } = 200;
        public T Data { get; set; }
        public string TraceId { get; set; } = string.Empty;
    }
}
