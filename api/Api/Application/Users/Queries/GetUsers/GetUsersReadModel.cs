namespace Application.Users.Queries.GetUsers
{
    public sealed record GetUsersReadModel(
        Guid Id,
        string UserName,
        string? AvatarUrl = null,
        bool IsInvited = false
    );
}
