using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApiDTOs
{
    /// <summary>
    /// Represents the data required for a user to log in using their username/mobile and password.
    /// </summary>
    public class LoginToApiViaUserMobilePass
    {
        private string _usernameOrMobile;

        /// <summary>
        /// The user's registered username or mobile number.
        /// </summary>
        [Display(Name = "نام کاربری یا شماره موبایل")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string UsernameOrMobile
        {
            get => _usernameOrMobile;
            set => _usernameOrMobile = value.ToEnglishNumber();
        }

        /// <summary>
        /// The user's password.
        /// </summary>
        [Display(Name = "رمز عبور")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Password { get; set; }

        /// <summary>
        /// A flag to indicate whether the user's session should be persisted.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
