using ChatCommunicator.Infrastructure.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatCommunicator.Infrastructure.Service

{
   public class TokenCleanupService : BackgroundService
    {
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenCleanupService> _logger;

        public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
                            _logger.LogInformation("[RemoveExpiredRefreshTokensAsync] {Count} expired tokens removed.", result);
                        }
                        else if (result == 0)
                        {
                            _logger.LogInformation("[RemoveExpiredRefreshTokensAsync] No expired tokens found.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[RemoveExpiredRefreshTokensAsync] An error occurred while removing expired tokens");
                    }
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task<int> RemoveFromDbExpiredRefreshTokensAsync(IUnitOfWork unitOfWork)
        {
            var result = await unitOfWork.RefreshTokens.WhereAsync(x => x.Expiration < DateTime.UtcNow);

            if (result.Any())
            {
                foreach (var refreshToken in result)
                {
                    unitOfWork.RefreshTokens.Delete(refreshToken);
                }
            }

            await unitOfWork.SaveAsync();

            return result.Count();
        }
    }
}
