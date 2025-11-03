namespace Domain.Entities
{
    public class User : BaseEntity
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
