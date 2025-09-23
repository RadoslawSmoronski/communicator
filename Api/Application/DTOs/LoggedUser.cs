namespace Application.DTOs
{
    public class LoggedUserDto
    {
        public required Guid Id { get; set; }
        public required string UserName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? AccessToken { get; set; }
        public Guid? RefreshToken { get; set; }
    }
}
