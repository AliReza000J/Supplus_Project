using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account
{
    public class CompleteProfile
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, long.MaxValue, ErrorMessage = ValidationMessages.IsRequired)]
        public long Id { get; set; }
        public string Fullname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RePassword { get; set; }
        public string? Email { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
    }

    public class CompleteProfileDto
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

        [EmailAddress(ErrorMessage = "ایمیل نامعتبر است.")]
        public string? Email { get; set; }

        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png" })]
        [MaxFileSize(1 * 1024 * 1024)]
        public IFormFile? ProfilePhoto { get; set; }
    }

}