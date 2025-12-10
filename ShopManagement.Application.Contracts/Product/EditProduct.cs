using _0_Framework.Application;
using Microsoft.AspNetCore.Http;

namespace ShopManagement.Application.Contracts.Product
{
    public class EditProduct : CreateProduct
    {
        public long Id { get; set; }

        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(2 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public new IFormFile? Picture { get; set; }
    }
}
