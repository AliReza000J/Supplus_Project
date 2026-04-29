using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApiDTOs
{
    /// <summary>
    /// Data transfer object for verifying an OTP and changing the user's mobile number.
    /// </summary>
    public class ChangeMobileDto : VerifyOtpDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user.
        /// </summary>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, long.MaxValue, ErrorMessage = ValidationMessages.IsRequired)]
        public long Id { get; set; }
    }
}
