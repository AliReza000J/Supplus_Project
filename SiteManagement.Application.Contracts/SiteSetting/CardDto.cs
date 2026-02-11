using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class CardDto
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string? Owner { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^\d{16}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string? Number { get; set; }
    }
}
