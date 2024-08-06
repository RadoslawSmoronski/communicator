using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.UserController
{
    public class RegisterResponseDto
    {
        public bool Succeeded { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public NewUserDto? User { get; set; }
    }
}
