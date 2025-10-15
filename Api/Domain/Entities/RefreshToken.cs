namespace Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public required Guid Token { get; set; }
        public required Guid UserId { get; set; }
        public DateTime Expiration {  get; set; } = DateTime.UtcNow; // refactor: change Expiration to CreatedBy
    }
}
