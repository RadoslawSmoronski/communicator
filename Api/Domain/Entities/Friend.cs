namespace Domain.Entities
{
    public class Friend : User
    {
        public required DateTime FriendshipCreatedAt { get; set; }
    }
}
