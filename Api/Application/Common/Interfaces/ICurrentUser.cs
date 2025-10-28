namespace Application.Common.Interfaces
{
    public interface ICurrentUser
    {
        Guid? Id { get; }
        List<string>? Roles { get; }
        bool IsInRole(string role);
    }
}
