using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.UserController
{
    public class NewUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
