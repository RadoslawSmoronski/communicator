using ChatCommunicator.Contracts.Chat;
using ChatCommunicator.Contracts.Dtos;
using ChatCommunicator.Contracts.Dtos.Chat;
using AutoMapper;
using ChatCommunicator.Contracts.Dtos.Controllers.UserController.RegisterAsync;

namespace ChatCommunicator.Contracts
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserAccount, SimpleUserDto>();
            CreateMap<UserAccount, RegisterDto>();
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ConversationId, opt => opt.MapFrom(src => src.ConversationId.ToString()))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId.ToString()));
        }
    }
}
