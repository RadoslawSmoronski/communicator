namespace Domain.Entities
{
    public class User
    {
        public required Guid Id { get; set; }
        public required string UserName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
