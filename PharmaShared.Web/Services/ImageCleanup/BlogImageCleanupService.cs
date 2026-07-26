using _0_Framework.Application;
using BlogManagement.Infrastructure.EFCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PharmaShared.Web.Services.ImageCleanup
{
    public class BlogImageCleanupService : IModuleImageCleanupService
    {
        private readonly BlogContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<BlogImageCleanupService> _logger;

        public BlogImageCleanupService(BlogContext db, IWebHostEnvironment env, ILogger<BlogImageCleanupService> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        public async Task<ImageCleanupResult> CleanupAsync(int keepRecentFilesForDays = 1)
        {
            var result = new ImageCleanupResult { ModuleName = "Blog Module" };
            var cutoffDate = DateTime.Now.AddDays(-keepRecentFilesForDays);

            var articleBodies = await _db.Articles
                .Where(a => !string.IsNullOrEmpty(a.Body))
                .Select(a => a.Body)
                .ToListAsync();

            var articleImages = await _db.Articles
                .Where(a => !string.IsNullOrEmpty(a.Picture))
                .Select(a => a.Picture)
                .ToListAsync();

            var categoryImages = await _db.ArticleCategories
                .Where(c => !string.IsNullOrEmpty(c.Picture))
                .Select(c => c.Picture)
                .ToListAsync();

            var pathsToCheck = new[]
            {
                Path.Combine(_env.WebRootPath, "Pictures", "Articles"),
                Path.Combine(_env.WebRootPath, "Pictures", "ArticleCategories")
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

                    bool isUsedInArticle = articleBodies.Any(b => b.Contains(fileName)) || articleImages.Any(b => b.Contains(fileName));
                    bool isUsedInCategory = categoryImages.Any(c => c.Contains(fileName));

                    if (!isUsedInArticle && !isUsedInCategory)
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
