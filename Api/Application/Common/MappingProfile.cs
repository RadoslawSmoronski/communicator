using Application.Users.Commands.RegisterUser;
using AutoMapper;
using Domain.Entities;

namespace Application.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, RegisterUserReadModel>()
                .ForCtorParam("Id", opt => opt.MapFrom(s => s.Id))
                .ForCtorParam("Email", opt => opt.MapFrom(s => s.Email))
                .ForCtorParam("UserName", opt => opt.MapFrom(s => s.UserName))
                .ForCtorParam("ConfirmToken", opt => opt.MapFrom(_ => (string?)null));
        }
    }
}
