using API.Contracts.Users.GetFriendInvitations;
using API.Contracts.Users.Register;
using Application.Users.Commands.RegisterUser;
using Application.Users.Queries.GetInvitations;
using AutoMapper;

namespace Application.Common
{
    public class ApiProfile : Profile
    {
        public ApiProfile()
        {
            CreateMap<RegisterUserReadModel, RegisterResponse>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.UserName));

            CreateMap<GetInvitationsReadModel, GetFriendshipInvitationResponse>()
                .ForMember(d => d.SenderUsername, o => o.MapFrom(s => s.SenderUserName));
        }
    }
}
