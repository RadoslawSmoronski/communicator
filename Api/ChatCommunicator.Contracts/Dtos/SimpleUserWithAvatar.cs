namespace ChatCommunicator.Contracts.Dtos
{
    public class SimpleUserWithAvatarDto
    {
        public required Guid Id { get; set; }
        public required string userName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
