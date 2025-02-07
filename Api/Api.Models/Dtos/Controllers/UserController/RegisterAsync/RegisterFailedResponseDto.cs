using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.UserController.RegisterAsync
{
    public class RegisterFailedResponseDto
    {
        public bool Succeeded { get; set; } = false;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
