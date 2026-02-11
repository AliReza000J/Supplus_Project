using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class BrandingSettingDto
    {
        public string? CurrentLogo { get; set; }
        public string? CurrentFavicon { get; set; }

        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png", ".svg", ".ico" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(1 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public IFormFile? LogoFile { get; set; }

        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png", ".svg", ".ico" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(1 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public IFormFile? FaviconFile { get; set; }
    }

}
