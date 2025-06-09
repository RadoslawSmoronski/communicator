using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatCommunicator.Models.Dtos.Controllers.UserController
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "RefreshToken is required.")]
        public Guid RefreshToken { get; set; } = Guid.Empty;
    }
}
