using Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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


    }
}
