namespace Domain.Entities
{
    public class Friend : User
    {
        public required Guid FriendshipId { get; set; }
        public required DateTime FriendshipCreatedAt { get; set; }
    }
}
