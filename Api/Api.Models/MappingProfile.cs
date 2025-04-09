using Api.Models.Chat;
using Api.Models.Dtos;
using Api.Models.Dtos.Chat;
using Api.Models.Dtos.Controllers.UserController;
using Api.Models.Dtos.Controllers.UserController.LoginAsync;
using Api.Models.Dtos.Responses.Interfaces;
using Api.Utilities.Result;
using AutoMapper;

namespace Api.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<HttpErrorType, ResponseHttpType>();
            CreateMap<UserAccount, SimpleUserDto>();
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ConversationId, opt => opt.MapFrom(src => src.ConversationId.ToString()))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId.ToString()));
        }
    }
}
