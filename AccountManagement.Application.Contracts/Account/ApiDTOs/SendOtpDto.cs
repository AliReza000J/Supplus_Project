using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApiDTOs
{
    /// <summary>
    /// Data transfer object for sending an OTP to a mobile number.
    /// </summary>
    public class SendOtpDto
    {
        private string _mobile;

        /// <summary>
        /// Gets or sets the mobile number. 
        /// Must match the standard Iranian mobile number format (e.g., 09xxxxxxxxx).
        /// </summary>
        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string Mobile
        {
            get => _mobile;
            set => _mobile = value.ToEnglishNumber();
        }
    }
}