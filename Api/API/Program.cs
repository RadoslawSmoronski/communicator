namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            // Add Swagger services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });

            builder.Services.AddOpenApi();
            builder.AddInfrastructureServices();
            builder.AddApplicationServices();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                // Enable Swagger and SwaggerUI in development
                app.UseSwagger();
                app.UseSwaggerUI();

                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
