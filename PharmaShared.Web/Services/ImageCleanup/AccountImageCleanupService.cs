using _0_Framework.Application;
using AccountManagement.Infrastructure.EFCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PharmaShared.Web.Services.ImageCleanup
{
    public class AccountImageCleanupService : IModuleImageCleanupService
    {
        private readonly AccountContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AccountImageCleanupService> _logger;

        public AccountImageCleanupService(AccountContext db, IWebHostEnvironment env, ILogger<AccountImageCleanupService> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        public async Task<ImageCleanupResult> CleanupAsync(int keepRecentFilesForDays = 1)
        {
            var result = new ImageCleanupResult { ModuleName = "Account Module" };
            var cutoffDate = DateTime.Now.AddDays(-keepRecentFilesForDays);

            var profilePictures = await _db.Accounts
                .Where(p => !string.IsNullOrEmpty(p.ProfilePhoto))
                .Select(p => p.ProfilePhoto)
                .ToListAsync();

            var pathsToCheck = new[]
            {
                Path.Combine(_env.WebRootPath, "Pictures", "ProfilePhotos"),
            };

            foreach (var path in pathsToCheck)
            {
                if (!Directory.Exists(path)) continue;

                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

                foreach (var filePath in files)
                {
                    result.CheckedFilesCount++;
                    var fileInfo = new FileInfo(filePath);

                    if (fileInfo.CreationTime > cutoffDate)
                        continue;

                    var fileName = fileInfo.Name;

                    bool isUsedInProfile = profilePictures.Any(p => p.Contains(fileName));

                    if (!isUsedInProfile)
                    {
                        try
                        {
                            File.Delete(filePath);
                            result.DeletedFilesCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete orphaned file in {Module}: {FilePath}", result.ModuleName, filePath);
                        }
                    }
                }
            }

            return result;
        }
    }
}
