namespace ChatCommunicator.Contracts
{
    public class ConfirmEmailDto
    {
        public required Guid UserId { get; set; }
        public required string Token { get; set; }
    }
}
