using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account
{
    public class SendPasswordResetLinkDto
    {
        [Display(Name = "ایمیل")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        public string Email { get; set; }
    }

}
