using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApplicationDTOs
{
    public class LoginViaUserPass
    {
        private string _username;

        [Display(Name = "نام کاربری")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Username
        {
            get => _username;
            set => _username = value.ToEnglishNumber();
        }

        [Display(Name = "کلمه عبور")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
