namespace Domain.Entities
{
    public class User : BaseEntity
    {
        public required string UserName { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
