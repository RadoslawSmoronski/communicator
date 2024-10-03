using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Data.Results.TokenService
{
    public class CreateAccessTokenResult
    {
        public string Token { get; set; } = string.Empty;
        public bool Succeeded { get; set; } = false;
        public string Message { get; set; } = String.Empty;
        public bool UserIdEmpty { get; set; } = false;


    }
}
