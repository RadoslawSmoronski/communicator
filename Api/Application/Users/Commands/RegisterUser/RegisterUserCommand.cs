using Application.Common.Security;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.RegisterUser
{
    public record RegisterUserCommand(string Email, string Username, string Password) : IRequest<Result<RegisterUserReadModel>>, IAllowAnonymous;
}
