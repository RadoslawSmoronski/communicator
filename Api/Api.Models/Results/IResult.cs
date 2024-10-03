using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Results
{
    public interface IResult
    {
        public bool Succeeded { get; set; }
        public string ErrorMessage { get; set; }
    }
}
