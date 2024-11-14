using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Dtos.Controllers.UserController
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "RefreshToken is required.")]
        [RegularExpression(@"^[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}$",
        ErrorMessage = "Not valid format.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
