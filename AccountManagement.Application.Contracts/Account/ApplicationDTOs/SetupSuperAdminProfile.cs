using _0_Framework.Application;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApplicationDTOs
{
    public class SetupSuperAdminProfile: EditSuperAdminProfile
    {
        [Display(Name = "کلید نصب")]
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string? InstallationKey { get; set; }
    }
}