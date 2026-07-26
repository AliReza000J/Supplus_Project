using _0_Framework.Application;
using Microsoft.AspNetCore.Mvc;

namespace PharmaAdmin.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IFileUploader _fileUploader;

        public MediaController(IFileUploader fileUploader)
        {
            _fileUploader = fileUploader;
        }

        [HttpPost("upload-tinymce")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromQuery] string moduleName, CancellationToken cancellation)
        {
            try
            {
                var folderName = string.IsNullOrWhiteSpace(moduleName) ? "General" : moduleName;

                var savedPath = await _fileUploader.UploadAsync(file, folderName, cancellation);

                if (string.IsNullOrEmpty(savedPath))
                    return BadRequest("آپلود فایل با خطا مواجه شد.");

                var imageUrl = $"/Pictures/{savedPath}";

                return Ok(new { location = imageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}