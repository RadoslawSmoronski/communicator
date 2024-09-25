using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.UsersController.GetUser
{
    public class GetUserResponseOkDto
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserAccount User { get; set; } = new UserAccount();
    }
}
