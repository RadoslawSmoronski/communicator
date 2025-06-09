using ChatCommunicator.Models.Chat;
using ChatCommunicator.Models.Dtos;
using ChatCommunicator.Models.Dtos.Chat;
using AutoMapper;

namespace ChatCommunicator.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserAccount, SimpleUserDto>();
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ConversationId, opt => opt.MapFrom(src => src.ConversationId.ToString()))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId.ToString()));
        }
    }
}
