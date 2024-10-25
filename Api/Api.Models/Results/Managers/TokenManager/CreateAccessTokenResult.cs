using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Results.Managers.TokenManager
{
    public class CreateAccessTokenResult : IResult
    {
        public string Token { get; set; } = string.Empty;
        public bool Succeeded { get; set; } = false;
        public string ErrorMessage { get; set; } = string.Empty;
        public bool UserIsNull { get; set; } = false;
        public bool UserUsernameIsNullOrEmpty { get; set; } = false;
        public bool UserIdIsNullOrEmpty { get; set; } = false;
        public bool UserDoesNotExist { get; set; } = false;
    }
}
