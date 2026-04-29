using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApiDTOs
{
    /// <summary>
    /// Represents the data required to verify a one-time password (OTP).
    /// </summary>
    public class VerifyOtpDto
    {
        private string _mobile;
        private string _code;

        /// <summary>
        /// The user's mobile number. Must be in Iranian format (e.g., 09123456789).
        /// </summary>
        [Display(Name = "شماره موبایل جدید")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string Mobile
        {
            get => _mobile;
            set => _mobile = value.ToEnglishNumber();
        }

        /// <summary>
        /// The verification code sent to the user's mobile number.
        /// </summary>
        [Display(Name = "کد تایید")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Code
        {
            get => _code;
            set => _code = value.ToEnglishNumber();
        }
    }
}
