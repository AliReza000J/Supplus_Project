using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Order
{
    public class ShipCommand
    {
        public long OrderId { get; set; }

        [Required(ErrorMessage =ValidationMessages.IsRequired)]
        public string PostTrackingNo { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public string Mobile { get; set; }
    }
}
