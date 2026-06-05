using System.ComponentModel.DataAnnotations;

namespace SiteManagement.Application.Contracts.SiteSetting
{
    public class FaqItemDto
    {
        [Required(ErrorMessage = "سوال الزامی است")]
        public string Question { get; set; }

        [Required(ErrorMessage = "پاسخ الزامی است")]
        public string Answer { get; set; }
    }
}
