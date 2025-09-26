namespace Application.DTOs
{
    public class RegisteredDto
    {
        public required Guid Id { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public string? ConfirmToken { get; set; } // to remove, only for development
    }
}
