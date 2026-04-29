using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApiDTOs
{
    /// <summary>
    /// Represents the data required to reset a user's password using a one-time password (OTP).
    /// </summary>
    public class ResetPasswordWithOtpApi : VerifyOtpDto
    {
        /// <summary>
        /// The user's new password.
        /// It must contain at least one uppercase letter, one lowercase letter, one digit, and be between 6 and 20 characters long.
        /// </summary>
        [Display(Name = "رمز عبور جدید")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [PasswordValidation(ErrorMessage = ValidationMessages.InvalidPass)]
        public string Password { get; set; }

        /// <summary>
        /// Confirmation of the new password. Must match the 'Password' field.
        /// </summary>
        [Display(Name = "تکرار رمز عبور جدید")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن مطابقت ندارند.")]
        public string ConfirmPassword { get; set; }
    }
}
