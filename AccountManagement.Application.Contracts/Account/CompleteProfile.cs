using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account
{
    public class CompleteProfile
    {
        public long Id { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Fullname { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Username { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Password { get; set; }

        [EmailAddress(ErrorMessage = "ایمیل نامعتبر است.")]
        public string? Email { get; set; }

        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png" })]
        [MaxFileSize(1 * 1024 * 1024)]
        public IFormFile? ProfilePhoto { get; set; }
    }

}