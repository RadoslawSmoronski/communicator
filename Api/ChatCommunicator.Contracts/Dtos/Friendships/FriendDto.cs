namespace ChatCommunicator.Contracts.Dtos.Friendships
{
    public class FriendDto
    {
        public required Guid Id { get; set; }
        public required Guid FriendshipId { get; set; }
        public required string userName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
