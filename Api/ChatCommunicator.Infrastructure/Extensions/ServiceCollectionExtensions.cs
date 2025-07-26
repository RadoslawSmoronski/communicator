
using ChatCommunicator.Infrastructure.Repository;
using ChatCommunicator.Infrastructure.Services;
using ChatCommunicator.Infrastructure.Services.Interfaces;
using ChatCommunicator.Infrastructure.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace ChatCommunicator.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChatCommunicatorInfrastructureLayer(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, ChatCommunicator.Infrastructure.UnitOfWork.UnitOfWork>();
            services.AddScoped<IFileStorageService, FileStorageService>();
            services.AddSingleton<LogCleanupService>();
            services.AddHostedService<LogCleanupService>();
            return services;
        }
    }
}