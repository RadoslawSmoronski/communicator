using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.UserController.RegisterAsync
{
    public class RegisterOkResponseDto
    {
        public bool Succeeded { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public RegisteredUserDto? User { get; set; }
    }
}
