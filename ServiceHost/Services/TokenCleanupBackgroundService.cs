using _0_Framework.Application;

namespace ServiceHost.Services
{
    public class TokenCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TokenCleanupBackgroundService> _logger;

        public TokenCleanupBackgroundService(IServiceScopeFactory scopeFactory,
                                             ILogger<TokenCleanupBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TokenCleanup background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var cleanupService = scope.ServiceProvider.GetRequiredService<ITokenCleanupService>();

                    var result = await cleanupService.CleanupAsync(deleteAfterDays: 7);

                    _logger.LogInformation(
                        "TokenCleanup completed. Expired Access: {0}, Revoked Refresh: {1}, Deleted: {2}",
                        result.ExpiredAccessTokens,
                        result.RevokedRefreshTokens,
                        result.DeletedTokens
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TokenCleanup error.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}