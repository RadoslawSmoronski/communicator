using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Models.Dtos.Controllers.UserController.LoginAsync;

namespace Api.Models.Dtos.Controllers.UserController
{
    public class DeleteResponseDto
    {
        public bool Succeeded { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public LoginDto? User { get; set; }
    }
}
