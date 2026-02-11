using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account
{
    public class RegisterByMobile
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = "فرمت وارد شده برای تلفن همراه نامعتبر است.")]
        public string Mobile { get; set; }
    }
}