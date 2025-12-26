using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Cart
{
    public class QtyDto
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(0, 200, ErrorMessage = ValidationMessages.InvalidInput)]
        public int Qty { get; set; }
    }
}
