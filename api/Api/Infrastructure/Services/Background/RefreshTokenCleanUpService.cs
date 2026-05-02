using Application.Repositories;
using Application.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Background
{
    public class RefreshTokenCleanUpService(
        IServiceProvider serviceProvider,
        ILogger<RefreshTokenCleanUpService> logger,
        IOptions<RefreshTokenSettings> settings)
        : BackgroundService
    {
        private readonly RefreshTokenSettings _settings = settings.Value;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<RefreshTokenCleanUpService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                while (!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        try
                        {
                            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                            var result = await RemoveFromDbExpiredRefreshTokensAsync(unitOfWork);

                            if (result > 0)
                            {
                                _logger.LogInformation("[RefreshTokenCleanUpService] {Count} expired tokens removed.", result);
                            }
                            else if (result == 0)
                            {
                                _logger.LogInformation("[RefreshTokenCleanUpService] No expired tokens found.");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[RefreshTokenCleanUpService] An error occurred while removing expired tokens");
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(_settings.RefreshTokenCleanUpIntercalInSeconds), stoppingToken);
                }
            }
            catch (TaskCanceledException) { }
        }

        private async Task<int> RemoveFromDbExpiredRefreshTokensAsync(IUnitOfWork unitOfWork)
        {
            var expirationTimeSpan = TimeSpan.FromSeconds(_settings.RefreshTokenLifeInSeconds);
            var expirationTime = DateTime.UtcNow - expirationTimeSpan;

            var result = await unitOfWork.RefreshTokens.WhereAsync(x => x.CreatedAt < expirationTime);

            if (result.Any())
            {
                unitOfWork.RefreshTokens.DeleteRange(result);
                await unitOfWork.SaveAsync();
            }

            return result.Count();
        }
    }
}
