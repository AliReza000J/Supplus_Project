using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Order
{
    public class OrderTrackingSmsDto
    {
        [Range(1,long.MaxValue)]
        public long OrderId { get; set; }

        [Required(ErrorMessage =ValidationMessages.IsRequired)]
        public string PostTrackingNo { get; set; }

        [RegularExpression(@"^09\d{9}$", ErrorMessage = ValidationMessages.InvalidInput)]
        public string Mobile { get; set; }
    }
}
