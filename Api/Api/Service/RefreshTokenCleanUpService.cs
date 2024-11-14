
namespace Api.Service
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);

        public TokenCleanupService()
        {
            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
