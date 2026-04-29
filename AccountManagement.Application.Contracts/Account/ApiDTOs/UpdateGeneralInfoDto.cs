using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApiDTOs
{
    /// <summary>
    /// Data transfer object for updating general user profile information.
    /// </summary>
    public class UpdateGeneralInfoDto
    {
        private string _username;

        /// <summary>
        /// Gets or sets the full name of the user. Maximum length is 100 characters.
        /// </summary>
        [Display(Name = "نام و نام خانوادگی")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(100, ErrorMessage = "نام کامل نمی‌تواند بیشتر از 100 کاراکتر باشد.")]
        public string Fullname { get; set; }

        /// <summary>
        /// Gets or sets the username. Maximum length is 50 characters.
        /// </summary>
        [Display(Name = "نام کاربری")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(50, ErrorMessage = "نام کاربری نمی‌تواند بیشتر از 50 کاراکتر باشد.")]
        public string Username
        {
            get => _username;
            set => _username = value.ToEnglishNumber();
        }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Display(Name = "ایمیل")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است.")]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the profile photo. Allowed formats are .jpeg, .jpg, and .png. Maximum size is 1 MB.
        /// </summary>
        [Display(Name = "عکس پروفایل")]
        [FileExtensionLimitation(new[] { ".jpeg", ".jpg", ".png" }, ErrorMessage = ValidationMessages.InvalidFileFormat)]
        [MaxFileSize(1 * 1024 * 1024, ErrorMessage = ValidationMessages.MaxFileSize)]
        public IFormFile? ProfilePhoto { get; set; }
    }
}