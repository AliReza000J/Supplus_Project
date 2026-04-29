using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Order
{
    public class RejectPaymentManually
    {
        [Range(1, long.MaxValue, ErrorMessage = ValidationMessages.IsRequired)]
        public long OrderId { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string? SelectedReason { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [MaxLength(80, ErrorMessage = ValidationMessages.MaxLength)]
        public string? ReasonDescription { get; set; }

        [Range(1, 999, ErrorMessage = ValidationMessages.IsRequired)]
        public long AdminId { get; set; }
    }
}
