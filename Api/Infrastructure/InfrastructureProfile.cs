using Application.Contracts.Chat;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Entities;

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

            CreateMap<Friendship, FriendshipEntity>()
                .ForMember(dest => dest.User1, opt => opt.MapFrom(src => src.User1))
                .ForMember(dest => dest.User2, opt => opt.MapFrom(src => src.User2));
            CreateMap<FriendshipEntity, Friendship>()
                .ForMember(dest => dest.User1, opt => opt.MapFrom(src => src.User1))
                .ForMember(dest => dest.User2, opt => opt.MapFrom(src => src.User2));

            CreateMap<Conversation, ConversationEntity>()
                .ForMember(dest => dest.User1, opt => opt.MapFrom(src => src.User1))
                .ForMember(dest => dest.User2, opt => opt.MapFrom(src => src.User2));
            CreateMap<ConversationEntity, Conversation>()
                .ForMember(dest => dest.User1, opt => opt.MapFrom(src => src.User1))
                .ForMember(dest => dest.User2, opt => opt.MapFrom(src => src.User2));

            CreateMap<Message, MessageEntity>()
                .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender));
            CreateMap<MessageEntity, Message>()
                .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.Sender));

            CreateMap<Message, MessageReceivedEvent>()
                .ForCtorParam("MessageId", opt => opt.MapFrom(s => s.Id))
                .ForCtorParam("ConversationId", opt => opt.MapFrom(s => s.ConversationId))
                .ForCtorParam("SenderId", opt => opt.MapFrom(s => s.SenderId))
                .ForCtorParam("Content", opt => opt.MapFrom(s => s.Content))
                .ForCtorParam("Timestamp", opt => opt.MapFrom(s => s.CreatedAt));
        }
    }
}