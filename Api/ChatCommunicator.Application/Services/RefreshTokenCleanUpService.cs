using ChatCommunicator.Application.Services.Interfaces;
using ChatCommunicator.Shared.Result;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChatCommunicator.Application.Service

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
                    var tokenManager = scope.ServiceProvider.GetRequiredService<ITokenService>();
                    var result = await tokenManager.RemoveExpiredRefreshTokensAsync();

                    if (result.IsSuccess)
                    {
                        Console.WriteLine($"[RemoveExpiredRefreshTokensAsync] {result.Value} expired tokens removed.");
                    }
                    else if (result.Error!.ErrorType == ErrorType.NotFound)
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
