using _0_Framework.Application;
using AccountManagement.Application.Contracts.Role;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApplicationDTOs
{

    public class CreateAccount
    {
        private string _username;
        private string _mobile;

        [Display(Name = "نام و نام خانوادگی")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Fullname { get; set; }

        [Display(Name = "نام کاربری")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Username
        {
            get => _username;
            set => _username = value.ToEnglishNumber();
        }

        [Display(Name = "کلمه عبور")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [PasswordValidation(ErrorMessage = ValidationMessages.InvalidPass)]
        public string Password { get; set; }

        [Display(Name = "تکرار کلمه عبور")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Compare(nameof(Password), ErrorMessage = ApplicationMessages.PasswordsNotMatch)]
        public string RePassword { get; set; }

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string Mobile
        {
            get => _mobile;
            set => _mobile = value.ToEnglishNumber();
        }

        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "ایمیل نامعتبر است.")]
        public string? Email { get; set; }

        [Display(Name = "نقش")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(2, 5, ErrorMessage = ValidationMessages.InvalidInput)]
        public long RoleId { get; set; } = 5;

        [Display(Name = "عکس پروفایل")]
        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(1 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public IFormFile? ProfilePhoto { get; set; }

        public List<RoleViewModel>? Roles { get; set; } = new();
    }
}