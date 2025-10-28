using Application.Common.Security;
using Application.DTOs;
using MediatR;
using Shared.Result;

namespace Application.Users.Commands.RegisterUser
{
    public record RegisterUserCommand(string Email, string Username, string Password) : IRequest<Result<RegisteredDto>>, IAllowAnonymous;
}
