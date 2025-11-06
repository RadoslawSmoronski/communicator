namespace API.Contracts.Auth.Login
{
    public sealed record LoginResponse(
    Guid Id,
    string Username,
    string? AvatarUrl = null,
    string? AccessToken = null,
    Guid? RefreshToken = null
);
}
