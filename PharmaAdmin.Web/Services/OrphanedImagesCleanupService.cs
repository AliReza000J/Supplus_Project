using _0_Framework.Application;

namespace PharmaShared.Web.Services
{
    public class OrphanedImagesBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OrphanedImagesBackgroundService> _logger;

        public OrphanedImagesBackgroundService(IServiceScopeFactory scopeFactory,
                                               ILogger<OrphanedImagesBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrphanedImages background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var cleanupServices = scope.ServiceProvider.GetServices<IModuleImageCleanupService>();

                    foreach (var service in cleanupServices)
                    {
                        var result = await service.CleanupAsync(keepRecentFilesForDays: 1);

                        _logger.LogInformation(
                            "ImageCleanup for [{Module}] completed. Checked: {Checked}, Deleted: {Deleted}",
                            result.ModuleName,
                            result.CheckedFilesCount,
                            result.DeletedFilesCount
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OrphanedImages cleanup error.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}