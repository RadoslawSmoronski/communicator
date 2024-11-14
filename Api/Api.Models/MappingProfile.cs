using Api.Models.Dtos.Controllers.UsersController.GetUsers;
using Api.Models.Dtos.Responses.Interfaces;
using Api.Utilities.Result;
using AutoMapper;

namespace Api.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserAccount, GetUsersUserResponseDto>();
            CreateMap<HttpErrorType, ResponseHttpType>();
        }
    }
}
