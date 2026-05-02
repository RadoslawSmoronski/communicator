namespace Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public required Guid Token { get; set; }
        public required Guid UserId { get; set; }
    }
}
