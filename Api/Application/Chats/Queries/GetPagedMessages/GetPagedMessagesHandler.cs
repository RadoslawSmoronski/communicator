using Application.Common.Interfaces;
using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Users;
using MediatR;
using Shared.Result;

namespace Application.Chats.Queries.GetPagedMessages
{
    public class GetPagedMessagesHandler : IRequestHandler<GetPagedMessagesQuery, Result<ExtendedPagedMessagesDto>>
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;

        public GetPagedMessagesHandler(IMessageService messageService, IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
        }

        public async Task<Result<ExtendedPagedMessagesDto>> Handle(GetPagedMessagesQuery request, CancellationToken cancellationToken)
        {
            if (!_userService.IsAuthorized(request.UserId))
            {
                return Error.Unauthorized("Unauthorized", "User is not authorized.");
            }

            return await _messageService.GetPagedMessagesFromMessageIdAsync(request.ConversationId, request.UserId, request.FromMessageId);
        }
    }
}
