using _0_Framework.Application;
using AccountManagement.Application.Contracts.Account.ApiDTOs;
using System.ComponentModel.DataAnnotations;

namespace AccountManagement.Application.Contracts.Account.ApplicationDTOs
{
    // Application DTO
    public class UpdateGeneralInfo : UpdateGeneralInfoDto
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, long.MaxValue, ErrorMessage = ValidationMessages.IsRequired)]
        public long Id { get; set; }
    }
}