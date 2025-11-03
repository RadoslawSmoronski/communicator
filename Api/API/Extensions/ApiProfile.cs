using API.Contracts.Users.Register;
using Application.Users.Commands.RegisterUser;
using AutoMapper;

namespace Application.Common
{
    public class ApiProfile : Profile
    {
        public ApiProfile()
        {
            CreateMap<RegisterUserReadModel, RegisterResponse>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.UserName));
        }
    }
}
