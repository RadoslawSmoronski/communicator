using API.Extensions;
using API.Hubs;
using Application.Common;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();
            builder.Services.AddControllers();

            builder.AddInfrastructureServices();
            builder.AddApplicationServices();
            builder.AddApiServices();
            builder.Services.AddApiProblemDetails();

            var app = builder.Build();

            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                // Enable Swagger and SwaggerUI in development
                app.UseSwagger();
                app.UseSwaggerUI();

                app.MapOpenApi();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseCors("AllowSpecificOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<ChatHub>("/chathub");

            app.Run();
        }
    }
}