using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApplicationDTOs
{
    public class AccountSearchModel
    {
        private string? _username;
        private string? _mobile;
        
        [Display(Name = "نام کاربری")]
        public string? Username
        {
            get => _username;
            set => _username = value.ToEnglishNumber();
        }

        [Display(Name = "شماره موبایل")]
        public string? Mobile
        {
            get => _mobile;
            set => _mobile = value.ToEnglishNumber();
        }

        public long RoleId { get; set; }
        public bool? IsActive { get; set; }
    }
}