using Api.Managers.Interfaces;
using Api.Utilities.Result;

namespace Api.Service

{
   public class TokenCleanupService : BackgroundService
    {
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);
        private readonly IServiceProvider _serviceProvider;

        public TokenCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var tokenManager = scope.ServiceProvider.GetRequiredService<ITokenManager>();
                    var result = await tokenManager.RemoveExpiredRefreshTokensAsync();

                    if (result.IsSuccess)
                    {
                        Console.WriteLine($"[RemoveExpiredRefreshTokensAsync] {result.Value} expired tokens removed.");
                    }
                    else if (result.Error!.ErrorType == HttpErrorType.NotFound)
                    {
                        Console.WriteLine("[RemoveExpiredRefreshTokensAsync] No expired tokens found.");
                    }
                    else
                    {
                        Console.WriteLine("[RemoveExpiredRefreshTokensAsync] An internal server error occurred.");
                    }
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
