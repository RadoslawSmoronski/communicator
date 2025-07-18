namespace ChatCommunicator.Contracts.Dtos.Controllers.UserController.LoginAsync
{
    public class LoggedUserDto
    {
        public required string UserName { get; set; }
        public required Guid Id { get; set; }
        public string? AvatarUrl { get; set; }
        public required string AccessToken { get; set; }
        public required Guid RefreshToken { get; set; }
    }
}
