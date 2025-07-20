namespace ChatCommunicator.Contracts
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required Guid Token { get; set; }
        public required Guid UserId { get; set; }
        public DateTime Expiration {  get; set; } = DateTime.MinValue;
    }
}
