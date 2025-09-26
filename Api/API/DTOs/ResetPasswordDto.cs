namespace API.DTOs
{
    public class ResetPasswordDto
    {
        public required Guid UserId { get; set; }
        public required string CodedToken { get; set; }
        public required string NewPassword { get; set; }
    }
}
