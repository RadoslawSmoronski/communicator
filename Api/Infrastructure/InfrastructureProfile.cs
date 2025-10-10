using Application.Common.Interfaces;
using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Services;

namespace Infrastructure
{
    public class InfrastructureProfile : Profile
    {
        public InfrastructureProfile()
        {
            CreateMap<UserAccount, User>();
            CreateMap<User, UserAccount>();
            CreateMap<FriendshipInvitation, FriendshipInvitationEntity>()
                .ForMember(dest => dest.SenderUser, opt => opt.MapFrom(src => src.SenderUser))
                .ForMember(dest => dest.RecipientUser, opt => opt.MapFrom(src => src.RecipientUser));
            CreateMap<FriendshipInvitationEntity, FriendshipInvitation>()
                .ForMember(dest => dest.SenderUser, opt => opt.MapFrom(src => src.SenderUser))
                .ForMember(dest => dest.RecipientUser, opt => opt.MapFrom(src => src.RecipientUser));
        }

    }
}