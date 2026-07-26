using _0_Framework.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShopManagement.Infrastructure.EFCore;

namespace PharmaShared.Web.Services.ImageCleanup
{
    public class ShopImageCleanupService : IModuleImageCleanupService
    {
        private readonly ShopContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ShopImageCleanupService> _logger;

        public ShopImageCleanupService(ShopContext db, IWebHostEnvironment env, ILogger<ShopImageCleanupService> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        public async Task<ImageCleanupResult> CleanupAsync(int keepRecentFilesForDays = 1)
        {
            var result = new ImageCleanupResult { ModuleName = "Shop Module" };
            var cutoffDate = DateTime.Now.AddDays(-keepRecentFilesForDays);

            var productBodies = await _db.Products
                .Where(p => !string.IsNullOrEmpty(p.Description))
                .Select(p => p.Picture)
                .ToListAsync();

            var productPictures = await _db.Products
                .Where(p => !string.IsNullOrEmpty(p.Picture))
                .Select(p => p.Picture)
                .ToListAsync();

            var productGalleryImages = await _db.ProductPictures
                .Where(p => !string.IsNullOrEmpty(p.Picture))
                .Select(p => p.Picture)
                .ToListAsync();

            var categoryImages = await _db.ProductCategories
                .Where(c => !string.IsNullOrEmpty(c.Picture))
                .Select(c => c.Picture)
                .ToListAsync();

            var slides = await _db.Slides
                .Where(c => !string.IsNullOrEmpty(c.Picture))
                .Select(c => c.Picture)
                .ToListAsync();

            var pathsToCheck = new[]
            {
                Path.Combine(_env.WebRootPath, "Pictures", "Products"),
                Path.Combine(_env.WebRootPath, "Pictures", "ProductCategories")
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

                    bool isUsedInProduct = productBodies.Any(b => b.Contains(fileName)) ||
                        productPictures.Any(p => p.Contains(fileName)) ||
                        productGalleryImages.Any(g => g.Contains(fileName));
                    bool isUsedInCategory = categoryImages.Any(c => c.Contains(fileName));
                    bool isUsedInSlides = slides.Any(s => s.Contains(fileName));

                    if (!isUsedInProduct && !isUsedInCategory && !isUsedInSlides)
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
