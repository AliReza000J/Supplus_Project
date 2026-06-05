using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApiDTOs
{
    public class SetPasswordApi
    {
        /// <summary>
        /// Gets or sets the new password. 
        /// Must contain at least one uppercase letter, one lowercase letter, one number, and be between 6 and 20 characters long.
        /// </summary>
        [Display(Name = "کلمه عبور جدید")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [PasswordValidation(ErrorMessage = "کلمه عبور باید شامل حداقل یک حرف بزرگ، حداقل یک حرف کوچک و حداقل یک رقم، حداقل 6 کاراکتر و حداکثر 20 کاراکتر باشد")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the confirmation of the new password. Must match the new password.
        /// </summary>
        [Display(Name = "تکرار کلمه عبور جدید")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Compare(nameof(Password), ErrorMessage = "کلمه عبور جدید و تکرار آن مطابقت ندارند.")]
        public string RePassword { get; set; }
    }
}
