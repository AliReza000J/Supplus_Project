using Microsoft.AspNetCore.Http;

namespace _0_Framework.Application
{
    public interface IFileUploader
    {
        Task<string> UploadAsync(IFormFile? file, string folder, CancellationToken cancellation);
        void DeleteFile(string? fileName);
        IFormFile GenerateIdenticonAsFormFile(string seed, int size = 200);
    }
}
