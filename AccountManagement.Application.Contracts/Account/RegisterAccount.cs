using _0_Framework.Application;
using AccountManagement.Application.Contracts.Role;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account
{
    public class RegisterAccount
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Fullname { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Username { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [PasswordValidation(ErrorMessage = "رمز عبور باید شامل حروف بزرگ، کوچک و عدد، حداقل 6 کاراکتر و حداکثر 20 کاراکتر باشد")]
        public string Password { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [PasswordValidation(ErrorMessage = "رمز عبور باید شامل حروف بزرگ، کوچک و عدد، حداقل 6 کاراکتر و حداکثر 20 کاراکتر باشد")]
        public string RePassword { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string Mobile { get; set; }

        [EmailAddress(ErrorMessage = "ایمیل نامعتبر است.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public virtual long RoleId { get; set; } = 5;

        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(1 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public IFormFile? ProfilePhoto { get; set; }

        public List<RoleViewModel>? Roles { get; set; } = new();
    }

    public class RegisterAccountApiDto : RegisterAccount
    {
        public override long RoleId
        {
            get => 5;
            set => base.RoleId = 5;
        }
    }
}