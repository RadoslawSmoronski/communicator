using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync
{
    public class LoggedUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public Guid Id { get; set; } = Guid.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public Guid RefreshToken { get; set; } = Guid.Empty;
    }
}
