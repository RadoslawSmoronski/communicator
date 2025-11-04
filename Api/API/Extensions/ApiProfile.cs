using API.Contracts.Users.GetChats;
using API.Contracts.Users.GetFriendInvitations;
using API.Contracts.Users.GetUsers;
using API.Contracts.Users.Register;
using Application.Users.Commands.RegisterUser;
using Application.Users.Queries.GetChats;
using Application.Users.Queries.GetInvitations;
using Application.Users.Queries.GetUsers;
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

            CreateMap<GetChatsReadModel, GetChatsResponse>()
                .ForMember(d => d.FriendUsername, o => o.MapFrom(s => s.FriendUserName));

            CreateMap<GetUsersReadModel, GetUsersResponse>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.UserName));
        }
    }
}
