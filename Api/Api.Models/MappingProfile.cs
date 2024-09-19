using Api.Models.Dtos.Controllers.UserController;
using Api.Models.Dtos.Controllers.UserController.LoginAsync;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserAccount, FindUserDto>();
            CreateMap<UserAccount, GetUserDto>();
        }
    }
}
