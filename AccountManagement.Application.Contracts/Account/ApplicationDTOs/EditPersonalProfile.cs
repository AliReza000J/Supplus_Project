using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApplicationDTOs
{
    public class EditPersonalProfile : UpdateGeneralInfo
    {
        private string _mobile;

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string Mobile
        {
            get => _mobile;
            set => _mobile = value.ToEnglishNumber();
        }
    }
}