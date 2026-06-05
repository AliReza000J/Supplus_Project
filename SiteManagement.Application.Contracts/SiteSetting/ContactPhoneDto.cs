using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class ContactPhoneDto
    {
        public string Title { get; set; }

        private string? _number;
        [RegularExpression(@"^0[1-9]\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string? Number
        {
            get => _number;
            set => _number = value?.ToEnglishNumber();
        }
    }
}
