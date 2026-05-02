namespace Infrastructure.Entities
{
    public class BaseEntityInfrastructure
    {
        public required Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
