using Api.Models.Dtos;
using AutoMapper;

namespace Api.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserAccount, SimpleUserDto>();
        }
    }
}
