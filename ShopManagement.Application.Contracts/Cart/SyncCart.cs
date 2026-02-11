using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace ShopManagement.Application.Contracts.Cart
{
    public class SyncCart
    {
        public string? CartKey { get; set; }
        public List<SyncCartItem>? Items { get; set; }
    }

    public class SyncCartItem
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, long.MaxValue, ErrorMessage = ValidationMessages.IsRequired)]
        public long ProductId { get; set; }

        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, 200, ErrorMessage = ValidationMessages.InvalidInput)]
        public int Qty { get; set; }
    }

    public class SyncCartDto
    {
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public List<SyncCartItem>? Items { get; set; }
    }
}