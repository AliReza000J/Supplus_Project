using System.ComponentModel.DataAnnotations;

namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class ContactSettingDto
    {
        public List<ContactPhoneDto> Phones { get; set; } = new();

        [EmailAddress(ErrorMessage = "ایمیل نامعتبر است.")]
        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? WorkingHours { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

    }
}
