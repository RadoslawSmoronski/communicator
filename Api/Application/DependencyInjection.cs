using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

    }
}
