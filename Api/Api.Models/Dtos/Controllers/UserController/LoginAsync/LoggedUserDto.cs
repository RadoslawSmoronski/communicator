using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.UserController.LoginAsync
{
    public class LoggedUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string AccessToken { get; set; } = String.Empty;
        public string RefreshToken {  get; set; } = String.Empty;
    }
}
