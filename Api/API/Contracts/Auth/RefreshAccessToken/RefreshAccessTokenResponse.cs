namespace API.Contracts.Auth.RefreshAccessToken
{
    public sealed record RefreshAccessTokenResponse(
        string AccessToken,
        Guid RefreshToken
    );
}
