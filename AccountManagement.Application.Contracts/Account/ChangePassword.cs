using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account
{
    public class ChangePassword
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, long.MaxValue, ErrorMessage = ValidationMessages.IsRequired)]
        public long Id { get; set; }
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [PasswordValidation(ErrorMessage = "رمز عبور باید شامل حروف بزرگ، کوچک و عدد، حداقل 6 کاراکتر و حداکثر 20 کاراکتر باشد")]
        public string Password { get; set; }
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [PasswordValidation(ErrorMessage = "رمز عبور باید شامل حروف بزرگ، کوچک و عدد، حداقل 6 کاراکتر و حداکثر 20 کاراکتر باشد")]
        public string RePassword { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [PasswordValidation(ErrorMessage = "رمز عبور باید شامل حروف بزرگ، کوچک و عدد، حداقل 6 کاراکتر و حداکثر 20 کاراکتر باشد")]
        public string Password { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [PasswordValidation(ErrorMessage = "رمز عبور باید شامل حروف بزرگ، کوچک و عدد، حداقل 6 کاراکتر و حداکثر 20 کاراکتر باشد")]
        public string RePassword { get; set; }
    }
}
