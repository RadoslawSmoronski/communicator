namespace API.DTOs
{
    public class ConfirmEmailDto
    {
        public required Guid UserId { get; set; }
        public required string ConfirmationToken { get; set; }
    }
}
