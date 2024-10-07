using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Utilities
{
    public class Result<TValue>
    {
        private readonly bool _isSuccess;
        private readonly TValue? _value;
        private readonly Error _error;

        private Result(TValue value)
        {
            _isSuccess = true;
            _value = value;
        }

        private Result(Error error)
        {
            _isSuccess = false;
            _value = default;
            _error = error;
        }

        public static Result<TValue> Success(TValue value)
            => new(value);
        public static Result<TValue> Failure(Error error)
            => new(error);

    }

    public record Error(string Code, string Description)
    {
        public static Error None => new(string.Empty, string.Empty);
    }

}
