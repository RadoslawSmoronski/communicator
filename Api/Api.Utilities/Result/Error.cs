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
            ErrorType errorType
        )
        {
            Code = code;
            Description = description;
            ErrorType = errorType;
        }

        public string Code { get; }

        public string Description { get; }

        public ErrorType ErrorType { get; }

        public static Error NotFound(string code, string description) =>
            new(code, description, ErrorType.NotFound);

        public static Error Validation(string code, string description) =>
            new(code, description, ErrorType.Validation);

        public static Error Conflict(string code, string description) =>
            new(code, description, ErrorType.Conflict);

        public static Error Unauthorized(string code, string description) =>
            new(code, description, ErrorType.Unauthorized);

        public static Error InternalServerError(string code, string description) =>
            new(code, description, ErrorType.InternalServerError);
    }
}
