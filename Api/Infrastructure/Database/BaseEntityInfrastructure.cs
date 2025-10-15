namespace Infrastructure.Database
{
    public class BaseEntityInfrastructure
    {
        public required Guid Id { get; set; } = Guid.NewGuid();
    }
}
