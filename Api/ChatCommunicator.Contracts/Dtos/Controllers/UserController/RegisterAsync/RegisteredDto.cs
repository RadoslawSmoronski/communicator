namespace ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync
{
    public class RegisteredDto
    {
        public required Guid Id { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string ConfirmToken { get; set; }
    }

}
