using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account
{
    public class ChangeEmailAdmin
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, long.MaxValue, ErrorMessage = ValidationMessages.IsRequired)]
        public long Id { get; set; }

        [Display(Name = "ایمیل جدید")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        public string Email { get; set; }

        [Display(Name = "تکرار ایمیل جدید")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        [Compare(nameof(Email), ErrorMessage = "ایمیل جدید و تکرار آن مطابقت ندارند.")]
        public string ReEmail { get; set; }
    }

}
