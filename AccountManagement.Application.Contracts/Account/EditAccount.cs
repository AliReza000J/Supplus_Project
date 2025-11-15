using _0_Framework.Application;
using Microsoft.AspNetCore.Http;

namespace AccountManagement.Application.Contracts.Account
{
    public class EditAccount : RegisterAccount
    {
        public long Id { get; set; }

        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(1 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public new IFormFile? ProfilePhoto { get; set; }
    }
}
