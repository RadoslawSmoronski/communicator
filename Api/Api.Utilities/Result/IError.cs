using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Utilities.Result
{
    public interface IError<T>
    {
        public string Code { get; }
        public string Description { get; }
        public T ErrorType { get; }


    }
}
