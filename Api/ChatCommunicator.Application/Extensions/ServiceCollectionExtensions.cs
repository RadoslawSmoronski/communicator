using ChatCommunicator.Application.Managers;
using ChatCommunicator.Application.Managers.Interfaces;
using ChatCommunicator.Application.Services;
using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Infrastructure.Service;
using Microsoft.Extensions.DependencyInjection;

namespace ChatCommunicator.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChatCommunicatorApplicationLayer(this IServiceCollection services)
        {
            services.AddSingleton<TokenCleanupService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IFriendsService, FriendsService>();
            services.AddScoped<IAccountManager, AccountManager>();
            services.AddSingleton<IUsersConnectionService, UsersConnectionService>();
            services.AddHostedService<TokenCleanupService>();
            services.AddScoped<IChatFriendsService, ChatFriendsService>();
            services.AddScoped<IUserAvatarService, UserAvatarService>();
            return services;
        }
    }
}