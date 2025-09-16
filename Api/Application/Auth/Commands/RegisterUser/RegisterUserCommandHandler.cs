using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Auth.Commands.RegisterUser
{
    public class RegisterUserHandler() : IRequestHandler<RegisterUserCommand, Guid>
    {

        public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            return Guid.NewGuid();
        }
    }
}
