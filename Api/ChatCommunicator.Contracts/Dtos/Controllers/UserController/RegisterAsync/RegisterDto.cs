using System.ComponentModel.DataAnnotations;

namespace ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(25, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public required string UserName { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string Password { get; set; }
    }

}
