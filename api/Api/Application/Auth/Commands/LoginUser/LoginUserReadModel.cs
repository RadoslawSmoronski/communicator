namespace Application.Auth.Commands.LoginUser
{
    public sealed record LoginUserReadModel(
        Guid Id,
        string UserName,
        string? AvatarUrl = null,
        string? AccessToken = null,
        Guid? RefreshToken = null
    );
}
