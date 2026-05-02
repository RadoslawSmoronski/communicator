namespace Application.Auth.Commands.RequestPasswordReset
{
    public record RequestPasswordResetReadModel(
        Guid UserId,
        string Token
        );
}