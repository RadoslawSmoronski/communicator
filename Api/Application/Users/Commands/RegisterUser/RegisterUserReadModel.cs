namespace Application.Users.Commands.RegisterUser
{
    public sealed record RegisterUserReadModel(
        Guid Id,
        string Email,
        string UserName,
        string? ConfirmToken = null
    );
}
