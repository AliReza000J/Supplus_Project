using _0_Framework.Application;
using AccountManagement.Application.Contracts.Role;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApplicationDTOs
{
    public class EditAccount : EditPersonalProfile
    {
        [Display(Name = "نقش")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(2, 5, ErrorMessage = ValidationMessages.InvalidInput)]
        public long RoleId { get; set; } = 5;

        public List<RoleViewModel>? Roles { get; set; } = new();
    }
}
