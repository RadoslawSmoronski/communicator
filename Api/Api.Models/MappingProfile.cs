using Api.Models.Dtos;
using Api.Models.Dtos.Controllers.UsersController.GetUsers;
using AutoMapper;

namespace Api.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserAccount, GetUsersUserResponseDto>();
            CreateMap<UserAccount, SimpleUserDto>();
        }
    }
}
