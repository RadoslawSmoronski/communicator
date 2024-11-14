using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Utilities.Result
{
    public class Error
    {
        private Error(
            string code,
            string description,
            HttpErrorType errorType
        )
        {
            Code = code;
            Description = description;
            ErrorType = errorType;
        }

        public string Code { get; }

        public string Description { get; }

        public HttpErrorType ErrorType { get; }

        public static Error NotFound(string code, string description) =>
            new(code, description, HttpErrorType.NotFound);

        public static Error BadRequest(string code, string description) =>
            new(code, description, HttpErrorType.BadRequest);

        public static Error Conflict(string code, string description) =>
            new(code, description, HttpErrorType.Conflict);

        public static Error Unauthorized(string code, string description) =>
            new(code, description, HttpErrorType.Unauthorized);

        public static Error InternalServerError(string code, string description) =>
            new(code, description, HttpErrorType.InternalServerError);
    }
}
