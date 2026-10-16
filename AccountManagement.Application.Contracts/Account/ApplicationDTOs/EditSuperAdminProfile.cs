using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AccountManagement.Application.Contracts.Account.ApplicationDTOs
{
    public class EditSuperAdminProfile
    {
        private string _mobile;

        [JsonIgnore]
        public long Id { get; } = 1;

        [Display(Name = "ایمیل")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        public string Email { get; set; }

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string Mobile
        {
            get => _mobile;
            set => _mobile = value.ToEnglishNumber();
        }

        [Display(Name = "کلمه عبور")]
        [PasswordValidation(ErrorMessage = ValidationMessages.InvalidPass)]
        public string? Password { get; set; }

        [Display(Name = "تکرار کلمه عبور")]
        [Compare(nameof(Password), ErrorMessage = ApplicationMessages.PasswordsNotMatch)]
        public string? RePassword { get; set; }
    }
}