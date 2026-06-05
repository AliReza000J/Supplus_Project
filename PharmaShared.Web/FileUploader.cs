using _0_Framework.Application;
using Jdenticon;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PharmaShared.Web
{
    public class FileUploader : IFileUploader
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FileUploader> _logger;

        public FileUploader(ILogger<FileUploader> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<string> UploadAsync(IFormFile? file, string folder, CancellationToken cancellation)
        {
            if (file == null || file.Length == 0) return "";

            const long maxSize = 5 * 1024 * 1024; // 5MB
            if (file.Length > maxSize)
                throw new InvalidOperationException("File size exceeded");

            var allowedExtensions = new Dictionary<string, string[]>
            {
                [".jpg"] = new[] { "image/jpeg" },
                [".jpeg"] = new[] { "image/jpeg" },
                [".png"] = new[] { "image/png" },
                [".ico"] = new[] { "image/x-icon", "image/vnd.microsoft.icon" }
            };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.ContainsKey(extension))
                throw new InvalidOperationException("Invalid file format");

            if (!allowedExtensions[extension].Contains(file.ContentType))
                throw new InvalidOperationException("Invalid content type");

            var fileName = $"{Guid.NewGuid():N}{extension}";

            var basePath = _config["UploadSettings:BasePath"]
               ?? throw new InvalidOperationException("Upload path is not configured.");

            var directoryPath = Path.Combine(
                basePath,
                "Pictures",
                folder
            );

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var filePath = Path.Combine(directoryPath, fileName);

            await using var stream =
                new FileStream(filePath, FileMode.CreateNew,
                FileAccess.Write, FileShare.None,
                bufferSize: 81920, useAsync: true);

            _logger.LogInformation("File uploaded: {File} ({Size} bytes)", fileName, file.Length);

            await file.CopyToAsync(stream, cancellation);
            return $"{folder}/{fileName}";
        }

        public void DeleteFile(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var basePath = _config["UploadSettings:BasePath"]
               ?? throw new InvalidOperationException("Upload path is not configured.");

            var filePath = Path.Combine(basePath, "Pictures", fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ImageCleanup error.");
                }
            }
        }

        public IFormFile GenerateIdenticonAsFormFile(string seed, int size = 200)
        {
            var ms = new MemoryStream();
            Identicon.FromValue(seed, size).SaveAsPng(ms);
            ms.Position = 0;

            var file = new FormFile(ms, 0, ms.Length, "ProfilePhoto", $"{Guid.NewGuid()}.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };

            return file;
        }
    }
}
