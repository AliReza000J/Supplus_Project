using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class SocialItemDto
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string? PlatformName { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string? LinkOrId { get; set; }
    }
}