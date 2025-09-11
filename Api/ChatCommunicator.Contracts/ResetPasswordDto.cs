namespace ChatCommunicator.Contracts
{
    public class ResetPasswordDto
    {
        public required Guid UserId { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }
}
