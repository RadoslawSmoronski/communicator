namespace Application.Auth.Commands.RefreshAccessToken
{
    public sealed record RefreshAccessTokenReadModel(
        string AccessToken,
        Guid RefreshToken
    );
}
