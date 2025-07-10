using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace ChatCommunicator.Infrastructure.Services
{
    public class LogCleanupService : BackgroundService
    {
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(30);
        private readonly IConfiguration _configuration;
        private readonly ILogger<LogCleanupService> _logger;

        public LogCleanupService(IConfiguration configuration, ILogger<LogCleanupService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var connectionString = _configuration.GetConnectionString("DefaultConnection");
                    using var connection = new NpgsqlConnection(connectionString);
                    await connection.OpenAsync(stoppingToken);

                    var command = new NpgsqlCommand("DELETE FROM logs WHERE raise_date < NOW() - INTERVAL '30 days'", connection);
                    var affectedRows = await command.ExecuteNonQueryAsync(stoppingToken);

                    _logger.LogInformation("[LogCleanupService] {Count} old log rows deleted.", affectedRows);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[LogCleanupService] Error during log cleanup.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
