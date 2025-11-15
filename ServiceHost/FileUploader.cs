using _0_Framework.Application;
using Jdenticon;

namespace ServiceHost
{
    public class FileUploader : IFileUploader
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileUploader(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string Upload(IFormFile file, string path)
        {
            if (file == null) return "";

            var directoryPath = $"{_webHostEnvironment.WebRootPath}/Pictures/{path}";

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            var fileName = $"{DateTime.Now.ToFileName()}-{file.FileName}";
            var filePath = $"{directoryPath}/{fileName}";
            using var output = File.Create(filePath);
            file.CopyTo(output);
            return $"{path}/{fileName}";
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
