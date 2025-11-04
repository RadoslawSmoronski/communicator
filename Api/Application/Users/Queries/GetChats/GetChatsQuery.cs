using Application.Common.Security;
using Application.DTOs;
using MediatR;
using Shared.Result;

namespace Application.Users.Queries.GetChats
{
    public record GetChatsQuery(Guid UserId, bool OnlyFriends) : IRequest<Result<List<GetChatsReadModel>>>, IRequireSameUser
    {
        public Guid TargetUserId => UserId;
    };
}
