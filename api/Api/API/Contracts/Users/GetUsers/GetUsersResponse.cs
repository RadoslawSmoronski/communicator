namespace API.Contracts.Users.GetUsers
{
    public sealed record GetUsersResponse(
        Guid Id,
        string Username,
        string? AvatarUrl = null,
        bool IsInvited = false
    );
}
