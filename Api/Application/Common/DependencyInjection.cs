using Application.Common.Behaviours;
using Application.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Application.Common;
public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
        });

        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        // configuration
        builder.Services.Configure<JWTTokenSettings>(
            builder.Configuration.GetSection("JWTTokenSettings"));
        builder.Services.Configure<RefreshTokenSettings>(
            builder.Configuration.GetSection("RefreshTokenSettings"));
        builder.Services.Configure<SmtpEmailSettings>(
            builder.Configuration.GetSection("SmtpEmailSettings"));
        builder.Services.Configure<RecoveryPasswordMessageSettings>(
            builder.Configuration.GetSection("RecoveryPasswordMessageSettings"));
        builder.Services.Configure<ConfirmEmailMessageSettings>(
            builder.Configuration.GetSection("ConfirmEmailMessageSettings"));
        builder.Services.Configure<UserAvatarSettings>(
            builder.Configuration.GetSection("UserAvatarSettings"));
        builder.Services.Configure<MessagesSettings>(
            builder.Configuration.GetSection("MessagesSettings"));
    }
}
