using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account
{
    public class ResetPasswordWithLinkDto
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Token { get; set; }

        [Display(Name = "ایمیل")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Email { get; set; }

        [Display(Name = "کلمه عبور جدید")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [PasswordValidation(ErrorMessage = ValidationMessages.InvalidPass)]
        public string NewPassword { get; set; }

        [Display(Name = "تکرار کلمه عبور جدید")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Compare("NewPassword", ErrorMessage = ApplicationMessages.PasswordsNotMatch)]
        public string ConfirmNewPassword { get; set; }
    }
}
