using MediatR;

namespace Application.Auth.Commands.RegisterUser
{
    public record RegisterUserCommand(string Email, string Password) : IRequest<Guid>;
}
