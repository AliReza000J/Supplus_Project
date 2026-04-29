using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class ContactSettingDto
    {
        private string? _phone;
        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string? Phone
        {
            get => _phone;
            set => _phone = value?.ToEnglishNumber();
        }

        [EmailAddress(ErrorMessage = "ایمیل نامعتبر است.")]
        public string? Email { get; set; }

        public string? Address { get; set; }
    }

}
