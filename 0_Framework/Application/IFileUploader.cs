using Microsoft.AspNetCore.Http;

namespace _0_Framework.Application
{
    public interface IFileUploader
    {
        string Upload(IFormFile file, string path);
        IFormFile GenerateIdenticonAsFormFile(string seed, int size = 200);
    }
}
