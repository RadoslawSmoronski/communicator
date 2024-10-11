using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Utilities.Result
{
    public static class ResultExtensions
    {
        public static T Match<T>(
            this Result result,
            Func<T> onSuccess,
            Func<Error, T> onFailure)
        {
            return result.IsSuccess ? onSuccess() : onFailure(result.Error!);
        }

        public static T Match<T, TValue>(
            this ResultT<TValue> result,
            Func<TValue, T> onSuccess,
            Func<Error, T> onFailure)
        {
            return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error!);
        }
    }
}
