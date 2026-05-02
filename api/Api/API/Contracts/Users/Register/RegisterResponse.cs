namespace API.Contracts.Users.Register
{
    public sealed record RegisterResponse(
        Guid Id,
        string Email,
        string Username,
        string? ConfirmToken
    );
}
