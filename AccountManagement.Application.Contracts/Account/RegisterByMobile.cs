using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account
{
    public class RegisterByMobile
    {
        [Required]
        [RegularExpression(@"^09\d{9}$")]
        public string Mobile { get; set; }
    }
}