using Application.Interfaces;
using Application.Interfaces.Users;
using Application.Repositories;
using Infrastructure.Database;
using Infrastructure.Services;
using Infrastructure.Services;
using Infrastructure.Services.Background;
using Infrastructure.Services.Users;
using Infrastructure.UnitOfWork;
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
        if (connectionString == null || connectionString.Length == 0)
        {
            //Log.Fatal("[Program settings] ConnectionString is not configured or empty. Please check your configuration.");
            Environment.Exit(1);
        }
        builder.Services.AddDbContext<ApplicationDbContext>
            (options => options.UseNpgsql(connectionString));

        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IUserAvatarService, UserAvatarService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IEmailService, SmtpEmailService>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IFileStorageService, FileStorageService>();

        builder.Services.AddHostedService<RefreshTokenCleanUpService>();

        builder.Services.AddIdentity<UserAccount, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

            options.User.RequireUniqueEmail = false;
            options.SignIn.RequireConfirmedEmail = true;
        }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

    }
}
