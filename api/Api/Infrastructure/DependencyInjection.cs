using System.Globalization;
using Application.Common.Authorization;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Users;
using Application.Common.Settings.Identity;
using Application.Repositories;
using Infrastructure;
using Infrastructure.Authorization;
using Infrastructure.Database;
using Infrastructure.Entities;
using Infrastructure.Services;
using Infrastructure.Services.Background;
using Infrastructure.Services.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (connectionString is null || connectionString.Length == 0)
        {
            Environment.Exit(1);
        };
        
        builder.Services.AddDbContext<ApplicationDbContext>
            (options => options.UseNpgsql(connectionString));

        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(InfrastructureProfile).Assembly);
        });

        builder.Services.AddSingleton<IUsersConnectionService, UsersConnectionService>();

        builder.Services.AddScoped<IMessageService, MessageService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IUserAvatarService, UserAvatarService>();
        builder.Services.AddScoped<IFriendInvitationsService, FriendInvitationsService>();
        builder.Services.AddScoped<IFriendshipService, FriendshipService>();
        builder.Services.AddScoped<IConversationService, ConversationService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IEmailService, SmtpEmailService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IFileStorageService, FileStorageService>();

        builder.Services.AddScoped<IFriendInvitationAccess, FriendInvitationAccess>();
        builder.Services.AddScoped<IFriendshipAccess, FriendshipAccess>();
        builder.Services.AddScoped<IChatAccess, ChatAccess>();

        builder.Services.AddHostedService<RefreshTokenCleanUpService>();
        
        var identitySettings = new IdentitySettings();
        
        builder.Configuration
            .GetSection("IdentitySettings")
            .Bind(identitySettings);
        
        builder.Services.AddIdentity<UserAccount, ApplicationRole>(options =>
        {
             options.Password.RequireDigit = identitySettings.PasswordSettings.RequireDigit;
             options.Password.RequiredLength = identitySettings.PasswordSettings.RequiredLength;
             options.Password.RequireLowercase = identitySettings.PasswordSettings.RequireLowercase;
             options.Password.RequireUppercase = identitySettings.PasswordSettings.RequireUppercase;
             options.Password.RequireNonAlphanumeric = identitySettings.PasswordSettings.RequireNonAlphanumeric;
            
             options.Lockout.AllowedForNewUsers = identitySettings.LockoutSettings.AllowedForNewUsers;
             options.Lockout.MaxFailedAccessAttempts = identitySettings.LockoutSettings.MaxFailedAccessAttempts;
             options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(identitySettings.LockoutSettings.DefaultLockoutTimeSpan);
            
            options.User.RequireUniqueEmail = identitySettings.UserSettings.RequireUniqueEmail;
             options.SignIn.RequireConfirmedEmail = identitySettings.SignInSettings.RequireConfirmedAccount;
        }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

    }
}