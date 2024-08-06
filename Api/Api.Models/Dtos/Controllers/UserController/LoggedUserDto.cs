using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.UserController
{
    public class LoggedUserDto
    {
        public string UserName { get; set; } = String.Empty;
        public string Token { get; set; } = String.Empty;
    }
}
