using System.ComponentModel.DataAnnotations;

namespace ChatCommunicator.Contracts.Dtos.Controllers.UserController
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "RefreshToken is required.")]
        public required Guid RefreshToken { get; set; }
    }
}
