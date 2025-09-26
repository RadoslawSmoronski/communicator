namespace Application.DTOs
{
    public class PasswordResetToken
    {
        public required Guid UserId { get; set; }
        public required string Token { get; set; }
    }
}
