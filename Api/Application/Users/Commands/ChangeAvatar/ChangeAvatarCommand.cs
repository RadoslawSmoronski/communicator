using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Result;

namespace Application.Users.Commands.ChangeAvatar
{
    public record ChangeAvatarCommand(Guid UserId, IFormFile File) : IRequest<Result<string>>;
}
