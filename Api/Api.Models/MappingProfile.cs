using Api.Models.Dtos.Controllers.UserController;
using Api.Models.Dtos.Controllers.UserController.LoginAsync;
using Api.Models.Dtos.Controllers.UsersController.GetUsers;
using Api.Models.Dtos.Responses.Interfaces;
using Api.Utilities.Result;
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
            CreateMap<UserAccount, GetUsersUserResponseDto>();
            CreateMap<HttpErrorType, ResponseHttpType>();
        }
    }
}
