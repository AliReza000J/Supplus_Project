using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class TicketTitleItem
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string? TitleName { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, int.MaxValue, ErrorMessage = ValidationMessages.InvalidInput)]
        public int? NumberOfRow { get; set; }
    }
}
